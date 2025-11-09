using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Permissions;
using abp_obs_project.Teachers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Courses;

/// <summary>
/// Application service for Course operations
/// Orchestrates business logic between UI and Domain layers
/// Demonstrates navigation property mapping (Course -> Teacher)
/// </summary>
[Authorize(abp_obs_projectPermissions.Courses.Default)]
public class CourseAppService : ApplicationService, ICourseAppService
{
    private readonly ICourseRepository _courseRepository;
    private readonly CourseManager _courseManager;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IObsCacheService _cacheService;

    public CourseAppService(
        ICourseRepository courseRepository,
        CourseManager courseManager,
        ITeacherRepository teacherRepository,
        IObsCacheService cacheService)
    {
        _courseRepository = courseRepository;
        _courseManager = courseManager;
        _teacherRepository = teacherRepository;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Gets a paginated and filtered list of courses with teacher information
    /// Uses GetListWithNavigationPropertiesAsync for efficient loading
    /// </summary>
    public virtual async Task<PagedResultDto<CourseDto>> GetListAsync(GetCoursesInput input)
    {
        // If user doesn't have ViewAll, restrict to their own courses by email -> teacher
        var hasViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.ViewAll);
        if (!hasViewAll)
        {
            var email = CurrentUser.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                return new PagedResultDto<CourseDto>
                {
                    TotalCount = 0,
                    Items = new List<CourseDto>()
                };
            }

            var teacher = await _teacherRepository.FindByEmailAsync(email);
            if (teacher == null)
            {
                return new PagedResultDto<CourseDto>
                {
                    TotalCount = 0,
                    Items = new List<CourseDto>()
                };
            }

            input.TeacherId = teacher.Id;
        }

        // Use cache only for admin users with simple list requests (no filters, no pagination)
        // Teachers should NOT use cache because their data is filtered by TeacherId
        var isSimpleListRequest = hasViewAll &&
                                  string.IsNullOrWhiteSpace(input.FilterText) &&
                                  string.IsNullOrWhiteSpace(input.Name) &&
                                  string.IsNullOrWhiteSpace(input.Code) &&
                                  input.CreditsMin == null &&
                                  input.CreditsMax == null &&
                                  input.Status == null &&
                                  input.TeacherId == null &&
                                  input.SkipCount == 0;

        if (isSimpleListRequest)
        {
            var cachedResult = await _cacheService.GetOrSetAsync(
                ObsCacheKeys.Courses.List,
                async () =>
                {
                    var totalCount = await _courseRepository.GetCountAsync();
                    var items = await _courseRepository.GetListWithNavigationPropertiesAsync(
                        sorting: input.Sorting,
                        maxResultCount: input.MaxResultCount
                    );

                    return new PagedResultDto<CourseDto>
                    {
                        TotalCount = totalCount,
                        Items = items.Select(item =>
                        {
                            var dto = ObjectMapper.Map<Course, CourseDto>(item.Course);
                            dto.TeacherName = item.Teacher != null
                                ? $"{item.Teacher.FirstName} {item.Teacher.LastName}"
                                : null;
                            return dto;
                        }).ToList()
                    };
                }
            );

            return cachedResult!;
        }

        // For filtered/paginated requests, bypass cache
        var totalCount = await _courseRepository.GetCountAsync(
            input.FilterText,
            input.Name,
            input.Code,
            input.CreditsMin,
            input.CreditsMax,
            input.Status,
            input.TeacherId
        );

        var items = await _courseRepository.GetListWithNavigationPropertiesAsync(
            input.FilterText,
            input.Name,
            input.Code,
            input.CreditsMin,
            input.CreditsMax,
            input.Status,
            input.TeacherId,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        // Map courses with teacher information
        return new PagedResultDto<CourseDto>
        {
            TotalCount = totalCount,
            Items = items.Select(item =>
            {
                var dto = ObjectMapper.Map<Course, CourseDto>(item.Course);
                dto.TeacherName = item.Teacher != null
                    ? $"{item.Teacher.FirstName} {item.Teacher.LastName}"
                    : null;
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// Gets a single course by ID with teacher information
    /// </summary>
    public virtual async Task<CourseDto> GetAsync(Guid id)
    {
        var courseWithNavigation = await _courseRepository.GetWithNavigationPropertiesAsync(id);

        var dto = ObjectMapper.Map<Course, CourseDto>(courseWithNavigation.Course);
        dto.TeacherName = courseWithNavigation.Teacher != null
            ? $"{courseWithNavigation.Teacher.FirstName} {courseWithNavigation.Teacher.LastName}"
            : null;

        return dto;
    }

    /// <summary>
    /// Returns the list of courses that the current student is enrolled in.
    /// </summary>
    public virtual async Task<ListResultDto<CourseDto>> GetMyCoursesAsync()
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ListResultDto<CourseDto>(Array.Empty<CourseDto>());
        }

        // Find student by email
        var studentRepo = LazyServiceProvider.LazyGetRequiredService<Students.IStudentRepository>();
        var gradeRepo = LazyServiceProvider.LazyGetRequiredService<Grades.IGradeRepository>();

        var student = await studentRepo.FindByEmailAsync(email);
        if (student == null)
        {
            return new ListResultDto<CourseDto>(Array.Empty<CourseDto>());
        }

        var grades = await gradeRepo.GetListAsync(studentId: student.Id);
        var courseIds = grades.Select(g => g.CourseId).Distinct().ToList();
        if (!courseIds.Any())
        {
            return new ListResultDto<CourseDto>(Array.Empty<CourseDto>());
        }

        var courses = await _courseRepository.GetListAsync(x => courseIds.Contains(x.Id));
        var dtos = courses.Select(c => ObjectMapper.Map<Course, CourseDto>(c)).ToList();
        return new ListResultDto<CourseDto>(dtos);
    }

    /// <summary>
    /// Returns a paged and filtered list of the current student's courses.
    /// </summary>
    public virtual async Task<PagedResultDto<CourseDto>> GetMyCoursesAsync(GetMyCoursesInput input)
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new PagedResultDto<CourseDto>
            {
                TotalCount = 0,
                Items = new List<CourseDto>()
            };
        }

        var studentRepo = LazyServiceProvider.LazyGetRequiredService<Students.IStudentRepository>();
        var gradeRepo = LazyServiceProvider.LazyGetRequiredService<Grades.IGradeRepository>();

        var student = await studentRepo.FindByEmailAsync(email);
        if (student == null)
        {
            return new PagedResultDto<CourseDto>
            {
                TotalCount = 0,
                Items = new List<CourseDto>()
            };
        }

        var grades = await gradeRepo.GetListAsync(studentId: student.Id);
        var courseIds = grades.Select(g => g.CourseId).Distinct().ToList();
        if (!courseIds.Any())
        {
            return new PagedResultDto<CourseDto>
            {
                TotalCount = 0,
                Items = new List<CourseDto>()
            };
        }

        // Base list limited to student's courses
        var list = await _courseRepository.GetListAsync(x => courseIds.Contains(x.Id));

        // Filters
        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var s = input.FilterText.Trim();
            list = list.Where(c =>
                (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.Code) && c.Code.Contains(s, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }
        if (input.Status.HasValue)
        {
            list = list.Where(c => c.Status == input.Status.Value).ToList();
        }
        if (input.CreditsMin.HasValue)
        {
            list = list.Where(c => c.Credits >= input.CreditsMin.Value).ToList();
        }
        if (input.CreditsMax.HasValue)
        {
            list = list.Where(c => c.Credits <= input.CreditsMax.Value).ToList();
        }

        // Sorting
        IEnumerable<Course> sorted = list;
        var sorting = input.Sorting?.Trim();
        if (!string.IsNullOrEmpty(sorting))
        {
            var desc = sorting.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
            var key = desc ? sorting[..^5] : sorting;
            sorted = key switch
            {
                nameof(Course.Code) => desc ? list.OrderByDescending(x => x.Code) : list.OrderBy(x => x.Code),
                nameof(Course.Credits) => desc ? list.OrderByDescending(x => x.Credits) : list.OrderBy(x => x.Credits),
                nameof(Course.CreationTime) => desc ? list.OrderByDescending(x => x.CreationTime) : list.OrderBy(x => x.CreationTime),
                _ => desc ? list.OrderByDescending(x => x.Name) : list.OrderBy(x => x.Name)
            };
        }
        else
        {
            sorted = list.OrderBy(x => x.Code);
        }

        var totalCount = sorted.Count();
        var paged = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        var dtos = paged.Select(c => ObjectMapper.Map<Course, CourseDto>(c)).ToList();

        return new PagedResultDto<CourseDto>
        {
            TotalCount = totalCount,
            Items = dtos
        };
    }

    /// <summary>
    /// Creates a new course using domain manager for business rules
    /// CourseManager validates teacher existence
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Create)]
    public virtual async Task<CourseDto> CreateAsync(CreateUpdateCourseDto input)
    {
        var course = await _courseManager.CreateAsync(
            input.Name,
            input.Code,
            input.Credits,
            input.TeacherId,
            input.Description,
            input.Status
        );

        // Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);

        return await MapCourseToDtoAsync(course);
    }

    /// <summary>
    /// Updates an existing course using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Edit)]
    public virtual async Task<CourseDto> UpdateAsync(Guid id, CreateUpdateCourseDto input)
    {
        var course = await _courseManager.UpdateAsync(
            id,
            input.Name,
            input.Code,
            input.Credits,
            input.TeacherId,
            input.Description,
            input.Status
        );

        // Invalidate cache after update
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);

        return await MapCourseToDtoAsync(course);
    }

    /// <summary>
    /// Deletes a course
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _courseRepository.DeleteAsync(id);

        // Invalidate cache after deletion
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);
    }

    /// <summary>
    /// Helper method to map course to DTO with teacher information
    /// </summary>
    private async Task<CourseDto> MapCourseToDtoAsync(Course course)
    {
        var dto = ObjectMapper.Map<Course, CourseDto>(course);

        if (course.TeacherId != Guid.Empty)
        {
            var teacher = await _teacherRepository.FindAsync(course.TeacherId);
            dto.TeacherName = teacher != null
                ? $"{teacher.FirstName} {teacher.LastName}"
                : null;
        }

        return dto;
    }
}
