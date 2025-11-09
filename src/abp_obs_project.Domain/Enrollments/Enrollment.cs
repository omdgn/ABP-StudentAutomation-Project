using JetBrains.Annotations;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Enrollment aggregate root entity
/// Represents a student's enrollment in a course
/// </summary>
public sealed class Enrollment : FullAuditedAggregateRoot<Guid>
{
    public Guid StudentId { get; private set; }

    public Guid CourseId { get; private set; }

    public DateTime EnrolledAt { get; private set; }

    public DateTime? WithdrawnAt { get; private set; }

    [NotNull]
    public EnumEnrollmentStatus Status { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    private Enrollment()
    {
    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Enrollment(
        Guid id,
        Guid studentId,
        Guid courseId) : base(id)
    {
        SetStudentId(studentId);
        SetCourseId(courseId);
        EnrolledAt = DateTime.Now;
        Status = EnumEnrollmentStatus.Active;
    }

    // Setter methods with validation
    public void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("SA-ENR-001")
                .WithData("Field", nameof(studentId));
        }
        StudentId = studentId;
    }

    public void SetCourseId(Guid courseId)
    {
        if (courseId == Guid.Empty)
        {
            throw new BusinessException("SA-ENR-002")
                .WithData("Field", nameof(courseId));
        }
        CourseId = courseId;
    }

    /// <summary>
    /// Withdraws the student from the course
    /// </summary>
    public void Withdraw()
    {
        if (Status == EnumEnrollmentStatus.Withdrawn)
        {
            throw new BusinessException("SA-ENR-003")
                .WithData("Message", "Student is already withdrawn from this course");
        }

        Status = EnumEnrollmentStatus.Withdrawn;
        WithdrawnAt = DateTime.Now;
    }

    /// <summary>
    /// Marks the enrollment as completed
    /// </summary>
    public void Complete()
    {
        if (Status == EnumEnrollmentStatus.Withdrawn)
        {
            throw new BusinessException("SA-ENR-004")
                .WithData("Message", "Cannot complete a withdrawn enrollment");
        }

        Status = EnumEnrollmentStatus.Completed;
    }

    /// <summary>
    /// Reactivates a withdrawn enrollment
    /// </summary>
    public void Reactivate()
    {
        if (Status != EnumEnrollmentStatus.Withdrawn)
        {
            throw new BusinessException("SA-ENR-005")
                .WithData("Message", "Can only reactivate withdrawn enrollments");
        }

        Status = EnumEnrollmentStatus.Active;
        WithdrawnAt = null;
    }
}
