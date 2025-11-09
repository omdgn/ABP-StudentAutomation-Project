using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.GlobalExceptions;
using abp_obs_project.Students;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Domain service for Enrollment entity
/// Handles business logic and validation
/// </summary>
public class EnrollmentManager(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    ICourseRepository courseRepository) : DomainService
{
    /// <summary>
    /// Creates a new enrollment with validation
    /// </summary>
    public async Task<Enrollment> CreateAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        // Business rule: Student must exist
        await CheckStudentExistsAsync(studentId, cancellationToken);

        // Business rule: Course must exist
        await CheckCourseExistsAsync(courseId, cancellationToken);

        // Business rule: Check for duplicate active enrollment
        await CheckDuplicateEnrollmentAsync(studentId, courseId, cancellationToken);

        // Create enrollment
        var enrollment = new Enrollment(
            GuidGenerator.Create(),
            studentId,
            courseId
        );

        return await enrollmentRepository.InsertAsync(enrollment, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Withdraws a student from a course
    /// </summary>
    public async Task<Enrollment> WithdrawAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Enrollment not found!", "SA-ENR-404", enrollment is null);

        enrollment.Withdraw();
        return await enrollmentRepository.UpdateAsync(enrollment, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Completes an enrollment
    /// </summary>
    public async Task<Enrollment> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Enrollment not found!", "SA-ENR-404", enrollment is null);

        enrollment.Complete();
        return await enrollmentRepository.UpdateAsync(enrollment, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Reactivates a withdrawn enrollment
    /// </summary>
    public async Task<Enrollment> ReactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Enrollment not found!", "SA-ENR-404", enrollment is null);

        enrollment.Reactivate();
        return await enrollmentRepository.UpdateAsync(enrollment, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Checks if student exists
    /// </summary>
    private async Task CheckStudentExistsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var studentExists = await studentRepository.FindAsync(studentId, cancellationToken: cancellationToken) != null;
        StudentAutomationException.ThrowIf(
            $"Student with ID '{studentId}' does not exist!",
            "SA-ENR-006",
            !studentExists
        );
    }

    /// <summary>
    /// Checks if course exists
    /// </summary>
    private async Task CheckCourseExistsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var courseExists = await courseRepository.FindAsync(courseId, cancellationToken: cancellationToken) != null;
        StudentAutomationException.ThrowIf(
            $"Course with ID '{courseId}' does not exist!",
            "SA-ENR-007",
            !courseExists
        );
    }

    /// <summary>
    /// Checks for duplicate active enrollment (same student and course)
    /// </summary>
    private async Task CheckDuplicateEnrollmentAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var existingEnrollment = await enrollmentRepository.FindByStudentAndCourseAsync(studentId, courseId, cancellationToken);

        if (existingEnrollment != null && existingEnrollment.Status == EnumEnrollmentStatus.Active)
        {
            StudentAutomationException.ThrowIf(
                $"Student is already enrolled in this course!",
                "SA-ENR-008",
                true
            );
        }
    }
}
