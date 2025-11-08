using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.GlobalExceptions;
using abp_obs_project.Teachers;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Courses;

/// <summary>
/// Domain service for Course aggregate
/// Handles business logic and validation
/// </summary>
public class CourseManager(
    ICourseRepository courseRepository,
    ITeacherRepository teacherRepository) : DomainService
{
    /// <summary>
    /// Creates a new course with validation
    /// </summary>
    public async Task<Course> CreateAsync(
        string name,
        string code,
        int credits,
        Guid teacherId,
        string? description = null,
        EnumCourseStatus status = EnumCourseStatus.NotStarted,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(name, code, credits, description);

        // Business rule: Course code must be unique
        await CheckCodeUniquenessAsync(code, null, cancellationToken);

        // Business rule: Teacher must exist
        await CheckTeacherExistsAsync(teacherId, cancellationToken);

        // Create course
        var course = new Course(
            GuidGenerator.Create(),
            name,
            code,
            credits,
            teacherId,
            description,
            status
        );

        return await courseRepository.InsertAsync(course, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing course with validation
    /// </summary>
    public async Task<Course> UpdateAsync(
        Guid id,
        string name,
        string code,
        int credits,
        Guid teacherId,
        string? description = null,
        EnumCourseStatus? status = null,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(name, code, credits, description);

        // Get course
        var course = await courseRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Course not found!", "SA-CRS-404", course is null);

        // Business rule: Course code must be unique
        await CheckCodeUniquenessAsync(code, id, cancellationToken);

        // Business rule: Teacher must exist
        await CheckTeacherExistsAsync(teacherId, cancellationToken);

        // Update course
        course.SetName(name);
        course.SetCode(code);
        course.SetCredits(credits);
        course.SetTeacherId(teacherId);
        course.SetDescription(description);

        if (status.HasValue)
        {
            course.SetStatus(status.Value);
        }

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            course.ConcurrencyStamp = concurrencyStamp;
        }

        return await courseRepository.UpdateAsync(course, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static void CheckValidate(
        string name,
        string code,
        int credits,
        string? description)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(name, nameof(name), CourseConsts.MaxNameLength, CourseConsts.MinNameLength);

        Check.NotNullOrWhiteSpace(code, nameof(code));
        Check.Length(code, nameof(code), CourseConsts.MaxCodeLength, CourseConsts.MinCodeLength);

        if (credits < CourseConsts.MinCredits || credits > CourseConsts.MaxCredits)
        {
            throw new BusinessException("SA-CRS-001")
                .WithData("MinCredits", CourseConsts.MinCredits)
                .WithData("MaxCredits", CourseConsts.MaxCredits);
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Check.Length(description, nameof(description), CourseConsts.MaxDescriptionLength);
        }
    }

    /// <summary>
    /// Checks if course code is unique
    /// </summary>
    private async Task CheckCodeUniquenessAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
    {
        var isUnique = await courseRepository.IsCodeUniqueAsync(code, excludeId, cancellationToken);
        StudentAutomationException.ThrowIf(
            $"Course code '{code}' is already in use!",
            "SA-CRS-006",
            !isUnique
        );
    }

    /// <summary>
    /// Checks if teacher exists
    /// </summary>
    private async Task CheckTeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacherExists = await teacherRepository.FindAsync(teacherId, cancellationToken: cancellationToken) != null;
        StudentAutomationException.ThrowIf(
            $"Teacher with ID '{teacherId}' does not exist!",
            "SA-CRS-007",
            !teacherExists
        );
    }
}
