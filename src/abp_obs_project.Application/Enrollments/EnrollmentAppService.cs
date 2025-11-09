using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.Permissions;
using abp_obs_project.Students;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Application service for Enrollment operations
/// Handles student course enrollment management
/// </summary>
[Authorize(abp_obs_projectPermissions.Enrollments.Default)]
public class EnrollmentAppService : ApplicationService, IEnrollmentAppService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly EnrollmentManager _enrollmentManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public EnrollmentAppService(
        IEnrollmentRepository enrollmentRepository,
        EnrollmentManager enrollmentManager,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _enrollmentManager = enrollmentManager;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Gets a paginated and filtered list of enrollments with student and course information
    /// </summary>
    public virtual async Task<PagedResultDto<EnrollmentDto>> GetListAsync(GetEnrollmentsInput input)
    {
        var totalCount = await _enrollmentRepository.GetCountAsync(
            input.FilterText,
            input.StudentId,
            input.CourseId,
            input.Status,
            input.EnrolledAtMin,
            input.EnrolledAtMax
        );

        // Get enrollments using simple query (no navigation properties needed for DTO)
        var enrollments = await _enrollmentRepository.GetListAsync(
            input.FilterText,
            input.StudentId,
            input.CourseId,
            input.Status,
            input.EnrolledAtMin,
            input.EnrolledAtMax,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        // Get related entities separately
        var studentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();
        var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();

        var students = new Dictionary<Guid, Students.Student>();
        var courses = new Dictionary<Guid, Courses.Course>();

        foreach (var studentId in studentIds)
        {
            var student = await _studentRepository.GetAsync(studentId);
            students[studentId] = student;
        }

        foreach (var courseId in courseIds)
        {
            var course = await _courseRepository.GetAsync(courseId);
            courses[courseId] = course;
        }

        var items = enrollments.Select(enrollment => new
        {
            Enrollment = enrollment,
            Student = students[enrollment.StudentId],
            Course = courses[enrollment.CourseId]
        }).ToList();

        return new PagedResultDto<EnrollmentDto>
        {
            TotalCount = totalCount,
            Items = items.Select(item =>
            {
                var dto = ObjectMapper.Map<Enrollment, EnrollmentDto>(item.Enrollment);
                dto.StudentName = $"{item.Student.FirstName} {item.Student.LastName}";
                dto.StudentNumber = item.Student.StudentNumber;
                dto.CourseName = item.Course.Name;
                dto.CourseCode = item.Course.Code;
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// Gets a single enrollment by ID with student and course information
    /// </summary>
    public virtual async Task<EnrollmentDto> GetAsync(Guid id)
    {
        var enrollment = await _enrollmentRepository.GetAsync(id);

        // Get related entities separately (avoid problematic GetWithNavigationPropertiesAsync)
        var student = await _studentRepository.GetAsync(enrollment.StudentId);
        var course = await _courseRepository.GetAsync(enrollment.CourseId);

        var dto = ObjectMapper.Map<Enrollment, EnrollmentDto>(enrollment);
        dto.StudentName = $"{student.FirstName} {student.LastName}";
        dto.StudentNumber = student.StudentNumber;
        dto.CourseName = course.Name;
        dto.CourseCode = course.Code;

        return dto;
    }

    /// <summary>
    /// Creates a new enrollment (enrolls a student in a course)
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Enrollments.Create)]
    public virtual async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input)
    {
        var enrollment = await _enrollmentManager.CreateAsync(
            input.StudentId,
            input.CourseId
        );

        return await MapEnrollmentToDtoAsync(enrollment);
    }

    /// <summary>
    /// Withdraws a student from a course
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Enrollments.Delete)]
    public virtual async Task<EnrollmentDto> WithdrawAsync(Guid id)
    {
        var enrollment = await _enrollmentManager.WithdrawAsync(id);
        return await MapEnrollmentToDtoAsync(enrollment);
    }

    /// <summary>
    /// Marks an enrollment as completed
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Enrollments.Edit)]
    public virtual async Task<EnrollmentDto> CompleteAsync(Guid id)
    {
        var enrollment = await _enrollmentManager.CompleteAsync(id);
        return await MapEnrollmentToDtoAsync(enrollment);
    }

    /// <summary>
    /// Reactivates a withdrawn enrollment
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Enrollments.Edit)]
    public virtual async Task<EnrollmentDto> ReactivateAsync(Guid id)
    {
        var enrollment = await _enrollmentManager.ReactivateAsync(id);
        return await MapEnrollmentToDtoAsync(enrollment);
    }

    /// <summary>
    /// Deletes an enrollment permanently
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Enrollments.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _enrollmentRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Checks if a student is enrolled in a course
    /// </summary>
    public virtual async Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid courseId)
    {
        return await _enrollmentRepository.IsStudentEnrolledAsync(studentId, courseId);
    }

    /// <summary>
    /// Helper method to map Enrollment entity to DTO with navigation properties
    /// </summary>
    private async Task<EnrollmentDto> MapEnrollmentToDtoAsync(Enrollment enrollment)
    {
        // Get related entities separately (avoid problematic GetWithNavigationPropertiesAsync)
        var student = await _studentRepository.GetAsync(enrollment.StudentId);
        var course = await _courseRepository.GetAsync(enrollment.CourseId);

        var dto = ObjectMapper.Map<Enrollment, EnrollmentDto>(enrollment);
        dto.StudentName = $"{student.FirstName} {student.LastName}";
        dto.StudentNumber = student.StudentNumber;
        dto.CourseName = course.Name;
        dto.CourseCode = course.Code;

        return dto;
    }
}
