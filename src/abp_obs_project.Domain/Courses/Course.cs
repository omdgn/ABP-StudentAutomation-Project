using JetBrains.Annotations;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Courses;

/// <summary>
/// Course aggregate root entity
/// Represents a course in the automation system
/// </summary>
public sealed class Course : FullAuditedAggregateRoot<Guid>
{
    [NotNull]
    public string Name { get; private set; } = string.Empty;

    [NotNull]
    public string Code { get; private set; } = string.Empty;

    public int Credits { get; private set; } 

    public string? Description { get; private set; }

    [NotNull]
    public EnumCourseStatus Status { get; private set; }

    public Guid TeacherId { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    private Course()
    {

    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Course(
        Guid id,
        string name,
        string code,
        int credits,
        Guid teacherId,
        string? description = null,
        EnumCourseStatus status = EnumCourseStatus.NotStarted) : base(id)
    {
        SetName(name);
        SetCode(code);
        SetCredits(credits);
        SetTeacherId(teacherId);
        SetDescription(description);
        SetStatus(status);
    }

    // Setter methods with validation
    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(name, nameof(name), CourseConsts.MaxNameLength, CourseConsts.MinNameLength);
        Name = name;
    }

    public void SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));
        Check.Length(code, nameof(code), CourseConsts.MaxCodeLength, CourseConsts.MinCodeLength);
        Code = code;
    }

    public void SetCredits(int credits)
    {
        if (credits < CourseConsts.MinCredits || credits > CourseConsts.MaxCredits)
        {
            throw new BusinessException("SA-CRS-001")
                .WithData("MinCredits", CourseConsts.MinCredits)
                .WithData("MaxCredits", CourseConsts.MaxCredits);
        }
        Credits = credits;
    }

    public void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            Check.Length(description, nameof(description), CourseConsts.MaxDescriptionLength);
        }
        Description = description;
    }

    public void SetStatus(EnumCourseStatus status)
    {
        Status = status;
    }

    public void SetTeacherId(Guid teacherId)
    {
        if (teacherId == Guid.Empty)
        {
            throw new BusinessException("SA-CRS-002")
                .WithData("Field", nameof(teacherId));
        }
        TeacherId = teacherId;
    }

    /// <summary>
    /// Starts the course
    /// </summary>
    public void Start()
    {
        if (Status != EnumCourseStatus.NotStarted)
        {
            throw new BusinessException("SA-CRS-003")
                .WithData("CurrentStatus", Status.ToString());
        }
        Status = EnumCourseStatus.InProgress;
    }

    /// <summary>
    /// Completes the course
    /// </summary>
    public void Complete()
    {
        if (Status != EnumCourseStatus.InProgress)
        {
            throw new BusinessException("SA-CRS-004")
                .WithData("CurrentStatus", Status.ToString());
        }
        Status = EnumCourseStatus.Completed;
    }

    /// <summary>
    /// Cancels the course
    /// </summary>
    public void Cancel()
    {
        if (Status == EnumCourseStatus.Completed)
        {
            throw new BusinessException("SA-CRS-005")
                .WithData("Message", "Cannot cancel a completed course");
        }
        Status = EnumCourseStatus.Cancelled;
    }
}
