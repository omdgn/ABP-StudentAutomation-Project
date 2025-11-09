using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Enrollment Application Service
/// Provides enrollment management operations
/// </summary>
public interface IEnrollmentAppService : IApplicationService
{
    /// <summary>
    /// Gets a paginated list of enrollments
    /// </summary>
    Task<PagedResultDto<EnrollmentDto>> GetListAsync(GetEnrollmentsInput input);

    /// <summary>
    /// Gets a single enrollment by ID
    /// </summary>
    Task<EnrollmentDto> GetAsync(Guid id);

    /// <summary>
    /// Creates a new enrollment (enrolls a student in a course)
    /// </summary>
    Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input);

    /// <summary>
    /// Withdraws a student from a course
    /// </summary>
    Task<EnrollmentDto> WithdrawAsync(Guid id);

    /// <summary>
    /// Marks an enrollment as completed
    /// </summary>
    Task<EnrollmentDto> CompleteAsync(Guid id);

    /// <summary>
    /// Reactivates a withdrawn enrollment
    /// </summary>
    Task<EnrollmentDto> ReactivateAsync(Guid id);

    /// <summary>
    /// Deletes an enrollment
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a student is enrolled in a course
    /// </summary>
    Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid courseId);
}
