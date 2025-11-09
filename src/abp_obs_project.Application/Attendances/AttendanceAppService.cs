using System;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.Permissions;
using abp_obs_project.Students;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Attendances;

/// <summary>
/// Application service for Attendance operations
/// Demonstrates date-based filtering and aggregate statistics
/// Shows transaction management for attendance tracking
/// </summary>
[Authorize(abp_obs_projectPermissions.Attendances.Default)]
public class AttendanceAppService : ApplicationService, IAttendanceAppService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly AttendanceManager _attendanceManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public AttendanceAppService(
        IAttendanceRepository attendanceRepository,
        AttendanceManager attendanceManager,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _attendanceRepository = attendanceRepository;
        _attendanceManager = attendanceManager;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Gets a paginated and filtered list of attendances with student and course information
    /// Supports date range filtering for attendance reports
    /// </summary>
    public virtual async Task<PagedResultDto<AttendanceDto>> GetListAsync(GetAttendancesInput input)
    {
        // Treat users who are real teachers (Courses.Default && !Courses.ViewAll) as scoped users
        var isTeacherScoped = (await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default))
                              && !(await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.ViewAll));

        // If not a scoped teacher and has Attendances.ViewAll, use global listing
        var hasAttendanceViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.ViewAll);
        if (!isTeacherScoped && hasAttendanceViewAll)
        {
            var totalCountAll = await _attendanceRepository.GetCountAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.AttendanceDateMin,
                input.AttendanceDateMax,
                input.IsPresent
            );

            var itemsAll = await _attendanceRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.AttendanceDateMin,
                input.AttendanceDateMax,
                input.IsPresent,
                input.Sorting,
                input.MaxResultCount,
                input.SkipCount
            );

            return new PagedResultDto<AttendanceDto>
            {
                TotalCount = totalCountAll,
                Items = itemsAll.Select(item =>
                {
                    var dto = ObjectMapper.Map<Attendance, AttendanceDto>(item.Attendance);
                    dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                    dto.CourseName = item.Course.Name;
                    return dto;
                }).ToList()
            };
        }

        // Otherwise, scope to the current teacher's courses
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new PagedResultDto<AttendanceDto>
            {
                TotalCount = 0,
                Items = Array.Empty<AttendanceDto>().ToList()
            };
        }

        var teacherRepo = LazyServiceProvider.LazyGetRequiredService<Teachers.ITeacherRepository>();
        var teacher = await teacherRepo.FindByEmailAsync(email);
        if (teacher == null)
        {
            return new PagedResultDto<AttendanceDto>
            {
                TotalCount = 0,
                Items = Array.Empty<AttendanceDto>().ToList()
            };
        }

        var teacherCourses = await _courseRepository.GetCoursesByTeacherIdAsync(teacher.Id);
        var allowedCourseIds = teacherCourses.Select(c => c.Id).ToHashSet();

        // If a specific courseId is provided but not allowed, return empty
        if (input.CourseId.HasValue && !allowedCourseIds.Contains(input.CourseId.Value))
        {
            return new PagedResultDto<AttendanceDto>
            {
                TotalCount = 0,
                Items = Array.Empty<AttendanceDto>().ToList()
            };
        }

        // If a specific (allowed) course is given, delegate to repo for paging
        if (input.CourseId.HasValue)
        {
            var totalCount = await _attendanceRepository.GetCountAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.AttendanceDateMin,
                input.AttendanceDateMax,
                input.IsPresent
            );

            var items = await _attendanceRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                input.CourseId,
                input.AttendanceDateMin,
                input.AttendanceDateMax,
                input.IsPresent,
                input.Sorting,
                input.MaxResultCount,
                input.SkipCount
            );

            return new PagedResultDto<AttendanceDto>
            {
                TotalCount = totalCount,
                Items = items.Select(item =>
                {
                    var dto = ObjectMapper.Map<Attendance, AttendanceDto>(item.Attendance);
                    dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                    dto.CourseName = item.Course.Name;
                    return dto;
                }).ToList()
            };
        }

        // No course filter: aggregate across teacher's courses and page in-memory
        var aggregated = new System.Collections.Generic.List<AttendanceWithNavigationProperties>();
        foreach (var courseId in allowedCourseIds)
        {
            var chunk = await _attendanceRepository.GetListWithNavigationPropertiesAsync(
                input.FilterText,
                input.StudentId,
                courseId,
                input.AttendanceDateMin,
                input.AttendanceDateMax,
                input.IsPresent,
                input.Sorting,
                int.MaxValue,
                0
            );
            aggregated.AddRange(chunk);
        }

        // Basic sorting support
        var sorted = aggregated.AsEnumerable();
        var sorting = input.Sorting?.Trim();
        if (!string.IsNullOrEmpty(sorting))
        {
            var desc = sorting.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
            var key = desc ? sorting[..^5] : sorting;
            sorted = key switch
            {
                nameof(Attendance.AttendanceDate) => desc ? aggregated.OrderByDescending(x => x.Attendance.AttendanceDate) : aggregated.OrderBy(x => x.Attendance.AttendanceDate),
                _ => desc ? aggregated.OrderByDescending(x => x.Course.Name).ThenByDescending(x => x.Student.LastName) : aggregated.OrderBy(x => x.Course.Name).ThenBy(x => x.Student.LastName)
            };
        }
        else
        {
            sorted = aggregated.OrderBy(x => x.Attendance.AttendanceDate).ThenBy(x => x.Course.Name).ThenBy(x => x.Student.LastName);
        }

        var total = sorted.Count();
        var paged = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new PagedResultDto<AttendanceDto>
        {
            TotalCount = total,
            Items = paged.Select(item =>
            {
                var dto = ObjectMapper.Map<Attendance, AttendanceDto>(item.Attendance);
                dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                dto.CourseName = item.Course.Name;
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// Gets a single attendance record by ID with student and course information
    /// </summary>
    public virtual async Task<AttendanceDto> GetAsync(Guid id)
    {
        var attendanceWithNavigation = await _attendanceRepository.GetWithNavigationPropertiesAsync(id);

        var dto = ObjectMapper.Map<Attendance, AttendanceDto>(attendanceWithNavigation.Attendance);
        dto.StudentName = $"{attendanceWithNavigation.Student.FirstName} {attendanceWithNavigation.Student.LastName}";
        dto.CourseName = attendanceWithNavigation.Course.Name;

        return dto;
    }

    /// <summary>
    /// Creates a new attendance record using domain manager for business rules
    /// AttendanceManager validates:
    /// - Student existence
    /// - Course existence
    /// - Date not in future
    /// - No duplicate attendance for same student-course-date
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Attendances.Create)]
    public virtual async Task<AttendanceDto> CreateAsync(CreateUpdateAttendanceDto input)
    {
        var attendance = await _attendanceManager.CreateAsync(
            input.StudentId,
            input.CourseId,
            input.AttendanceDate,
            input.IsPresent,
            input.Remarks
        );

        return await MapAttendanceToDtoAsync(attendance);
    }

    /// <summary>
    /// Updates an existing attendance record using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Attendances.Edit)]
    public virtual async Task<AttendanceDto> UpdateAsync(Guid id, CreateUpdateAttendanceDto input)
    {
        var attendance = await _attendanceManager.UpdateAsync(
            id,
            input.StudentId,
            input.CourseId,
            input.AttendanceDate,
            input.IsPresent,
            input.Remarks
        );

        return await MapAttendanceToDtoAsync(attendance);
    }

    /// <summary>
    /// Deletes an attendance record
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Attendances.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _attendanceRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Gets absence count for a specific student in a specific course
    /// Useful for attendance monitoring and alerts
    /// </summary>
    public virtual async Task<int> GetAbsenceCountAsync(Guid studentId, Guid courseId)
    {
        return await _attendanceRepository.GetAbsenceCountByStudentAndCourseAsync(studentId, courseId);
    }

    /// <summary>
    /// Gets attendance rate (percentage) for a specific student in a specific course
    /// Demonstrates aggregate query pattern
    /// </summary>
    public virtual async Task<double> GetAttendanceRateAsync(Guid studentId, Guid courseId)
    {
        return await _attendanceRepository.GetAttendanceRateByStudentAndCourseAsync(studentId, courseId);
    }

    /// <summary>
    /// Lists attendances for the current user (student).
    /// Requires only default permission; enforces self by CurrentUser.Email.
    /// </summary>
    public virtual async Task<ListResultDto<AttendanceDto>> GetMyAttendancesAsync()
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ListResultDto<AttendanceDto>(Array.Empty<AttendanceDto>());
        }

        var student = await _studentRepository.FindByEmailAsync(email);
        if (student == null)
        {
            return new ListResultDto<AttendanceDto>(Array.Empty<AttendanceDto>());
        }

        var list = await _attendanceRepository.GetAttendancesByStudentIdAsync(student.Id);
        var dtos = list.Select(a => ObjectMapper.Map<Attendance, AttendanceDto>(a)).ToList();
        return new ListResultDto<AttendanceDto>(dtos);
    }

    /// <summary>
    /// Helper method to map attendance to DTO with student and course information
    /// </summary>
    private async Task<AttendanceDto> MapAttendanceToDtoAsync(Attendance attendance)
    {
        var dto = ObjectMapper.Map<Attendance, AttendanceDto>(attendance);

        var student = await _studentRepository.GetAsync(attendance.StudentId);
        var course = await _courseRepository.GetAsync(attendance.CourseId);

        dto.StudentName = $"{student.FirstName} {student.LastName}";
        dto.CourseName = course.Name;

        return dto;
    }
}
