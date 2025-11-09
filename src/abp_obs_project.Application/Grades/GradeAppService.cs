using System;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.Events;
using abp_obs_project.Permissions;
using abp_obs_project.Students;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.Grades;

/// <summary>
/// Application service for Grade operations
/// Demonstrates multi-entity navigation (Grade -> Student + Course)
/// Shows transaction management and business rule enforcement
/// </summary>
[Authorize(abp_obs_projectPermissions.Grades.Default)]
public class GradeAppService : ApplicationService, IGradeAppService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly GradeManager _gradeManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDistributedEventBus _distributedEventBus;

    public GradeAppService(
        IGradeRepository gradeRepository,
        GradeManager gradeManager,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IDistributedEventBus distributedEventBus)
    {
        _gradeRepository = gradeRepository;
        _gradeManager = gradeManager;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _distributedEventBus = distributedEventBus;
    }

    /// <summary>
    /// Gets a paginated and filtered list of grades with student and course information
    /// Uses GetListWithNavigationPropertiesAsync for efficient eager loading
    /// </summary>
    public virtual async Task<PagedResultDto<GradeDto>> GetListAsync(GetGradesInput input)
    {
        // Treat users who are real teachers (Courses.Default && !Courses.ViewAll) as scoped users
        var isTeacherScoped = (await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default))
                              && !(await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.ViewAll));

        // If not a scoped teacher and has Grades.ViewAll, keep global listing
        var hasGradesViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.ViewAll);
        if (!isTeacherScoped && hasGradesViewAll)
        {
            var totalCountAll = await _gradeRepository.GetCountAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.GradeValueMin,
                input.GradeValueMax,
                input.Status
            );

            var itemsAll = await _gradeRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.GradeValueMin,
                input.GradeValueMax,
                input.Status,
                input.Sorting,
                input.MaxResultCount,
                input.SkipCount
            );

            return new PagedResultDto<GradeDto>
            {
                TotalCount = totalCountAll,
                Items = itemsAll.Select(item =>
                {
                    var dto = ObjectMapper.Map<Grade, GradeDto>(item.Grade);
                    dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                    dto.CourseName = item.Course.Name;
                    return dto;
                }).ToList()
            };
        }

        // Otherwise, scope to current teacher's courses by CurrentUser.Email
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new PagedResultDto<GradeDto>
            {
                TotalCount = 0,
                Items = Array.Empty<GradeDto>().ToList()
            };
        }

        var teacherRepo = LazyServiceProvider.LazyGetRequiredService<Teachers.ITeacherRepository>();
        var teacher = await teacherRepo.FindByEmailAsync(email);
        if (teacher == null)
        {
            return new PagedResultDto<GradeDto>
            {
                TotalCount = 0,
                Items = Array.Empty<GradeDto>().ToList()
            };
        }

        // Get teacher's courses
        var teacherCourses = await _courseRepository.GetCoursesByTeacherIdAsync(teacher.Id);
        var allowedCourseIds = teacherCourses.Select(c => c.Id).ToHashSet();

        // If client requested a specific course that is not allowed, return empty
        if (input.CourseId.HasValue && !allowedCourseIds.Contains(input.CourseId.Value))
        {
            return new PagedResultDto<GradeDto>
            {
                TotalCount = 0,
                Items = Array.Empty<GradeDto>().ToList()
            };
        }

        // If a specific course is provided (and allowed), we can delegate to repository for paging
        if (input.CourseId.HasValue)
        {
            var totalCount = await _gradeRepository.GetCountAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.GradeValueMin,
                input.GradeValueMax,
                input.Status
            );

            var items = await _gradeRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.GradeValueMin,
                input.GradeValueMax,
                input.Status,
                input.Sorting,
                input.MaxResultCount,
                input.SkipCount
            );

            return new PagedResultDto<GradeDto>
            {
                TotalCount = totalCount,
                Items = items.Select(item =>
                {
                    var dto = ObjectMapper.Map<Grade, GradeDto>(item.Grade);
                    dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                    dto.CourseName = item.Course.Name;
                    return dto;
                }).ToList()
            };
        }

        // No specific course: aggregate across teacher's courses and page in-memory
        var aggregated = new System.Collections.Generic.List<GradeWithNavigationProperties>();
        foreach (var courseId in allowedCourseIds)
        {
            var chunk = await _gradeRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                courseId,
                input.GradeValueMin,
                input.GradeValueMax,
                input.Status,
                input.Sorting,
                // get more than needed; we'll page after aggregation
                int.MaxValue,
                0
            );
            aggregated.AddRange(chunk);
        }

        // Basic sorting support (fallback to CourseName then StudentName)
        var sorted = aggregated.AsEnumerable();
        var sorting = input.Sorting?.Trim();
        if (!string.IsNullOrEmpty(sorting))
        {
            var desc = sorting.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
            var key = desc ? sorting[..^5] : sorting;
            sorted = key switch
            {
                nameof(Grade.GradeValue) => desc ? aggregated.OrderByDescending(x => x.Grade.GradeValue) : aggregated.OrderBy(x => x.Grade.GradeValue),
                nameof(Grade.CreationTime) => desc ? aggregated.OrderByDescending(x => x.Grade.CreationTime) : aggregated.OrderBy(x => x.Grade.CreationTime),
                _ => desc ? aggregated.OrderByDescending(x => x.Course.Name).ThenByDescending(x => x.Student.LastName) : aggregated.OrderBy(x => x.Course.Name).ThenBy(x => x.Student.LastName)
            };
        }
        else
        {
            sorted = aggregated.OrderBy(x => x.Course.Name).ThenBy(x => x.Student.LastName).ThenBy(x => x.Student.FirstName);
        }

        var total = sorted.Count();
        var paged = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new PagedResultDto<GradeDto>
        {
            TotalCount = total,
            Items = paged.Select(item =>
            {
                var dto = ObjectMapper.Map<Grade, GradeDto>(item.Grade);
                dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                dto.CourseName = item.Course.Name;
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// Gets a single grade by ID with student and course information
    /// </summary>
    public virtual async Task<GradeDto> GetAsync(Guid id)
    {
        var gradeWithNavigation = await _gradeRepository.GetWithNavigationPropertiesAsync(id);

        var dto = ObjectMapper.Map<Grade, GradeDto>(gradeWithNavigation.Grade);
        dto.StudentName = $"{gradeWithNavigation.Student.FirstName} {gradeWithNavigation.Student.LastName}";
        dto.CourseName = gradeWithNavigation.Course.Name;

        return dto;
    }

    /// <summary>
    /// Creates a new grade using domain manager for business rules
    /// GradeManager validates:
    /// - Student existence
    /// - Course existence
    /// - No duplicate grade for same student-course
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Grades.Create)]
    public virtual async Task<GradeDto> CreateAsync(CreateUpdateGradeDto input)
    {
        var grade = await _gradeManager.CreateAsync(
            input.StudentId,
            input.CourseId,
            input.GradeValue,
            input.Comments
        );

        // Get student and course info for event
        var student = await _studentRepository.GetAsync(grade.StudentId);
        var course = await _courseRepository.GetAsync(grade.CourseId);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new GradeCreatedEto
        {
            GradeId = grade.Id,
            StudentId = grade.StudentId,
            CourseId = grade.CourseId,
            GradeValue = grade.GradeValue,
            Status = grade.Status,
            GradedAt = grade.GradedAt ?? Clock.Now,
            StudentEmail = student.Email,
            StudentName = $"{student.FirstName} {student.LastName}",
            CourseName = course.Name
        });

        return await MapGradeToDtoAsync(grade);
    }

    /// <summary>
    /// Updates an existing grade using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Grades.Edit)]
    public virtual async Task<GradeDto> UpdateAsync(Guid id, CreateUpdateGradeDto input)
    {
        var grade = await _gradeManager.UpdateAsync(
            id,
            input.StudentId,
            input.CourseId,
            input.GradeValue,
            input.Comments
        );

        return await MapGradeToDtoAsync(grade);
    }

    /// <summary>
    /// Deletes a grade
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Grades.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _gradeRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Gets average grade for a specific student
    /// Demonstrates aggregate query pattern
    /// </summary>
    public virtual async Task<double> GetAverageGradeByStudentAsync(Guid studentId)
    {
        return await _gradeRepository.GetAverageGradeByStudentIdAsync(studentId);
    }

    /// <summary>
    /// Lists grades for the current user (student).
    /// Requires only default permission; enforces self by CurrentUser.Email.
    /// </summary>
    public virtual async Task<ListResultDto<GradeDto>> GetMyGradesAsync()
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ListResultDto<GradeDto>(Array.Empty<GradeDto>());
        }

        var student = await _studentRepository.FindByEmailAsync(email);
        if (student == null)
        {
            return new ListResultDto<GradeDto>(Array.Empty<GradeDto>());
        }

        var items = await _gradeRepository.GetListWithNavigationPropertiesAsync(
            studentId: student.Id,
            maxResultCount: int.MaxValue,
            skipCount: 0
        );

        var dtos = items.Select(item =>
        {
            var dto = ObjectMapper.Map<Grade, GradeDto>(item.Grade);
            dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
            dto.CourseName = item.Course.Name;
            return dto;
        }).ToList();

        return new ListResultDto<GradeDto>(dtos);
    }

    /// <summary>
    /// Helper method to map grade to DTO with student and course information
    /// </summary>
    private async Task<GradeDto> MapGradeToDtoAsync(Grade grade)
    {
        var dto = ObjectMapper.Map<Grade, GradeDto>(grade);

        var student = await _studentRepository.GetAsync(grade.StudentId);
        var course = await _courseRepository.GetAsync(grade.CourseId);

        dto.StudentName = $"{student.FirstName} {student.LastName}";
        dto.CourseName = course.Name;

        return dto;
    }
}
