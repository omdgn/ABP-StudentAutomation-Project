using JetBrains.Annotations;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Grades;

/// <summary>
/// Grade entity (not an aggregate root)
/// Represents a grade given to a student for a course
/// </summary>
public class Grade : FullAuditedEntity<Guid>
{
    public Guid StudentId { get; private set; }

    public Guid CourseId { get; private set; }

    public double GradeValue { get; private set; }

    [NotNull]
    public EnumGradeStatus Status { get; private set; }

    public string? Comments { get; private set; }

    public DateTime? GradedAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    protected Grade()
    {
        
    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Grade(
        Guid id,
        Guid studentId,
        Guid courseId,
        double gradeValue,
        string? comments = null) : base(id)
    {
        SetStudentId(studentId);
        SetCourseId(courseId);
        SetGradeValue(gradeValue);
        SetComments(comments);
        SetGradedAt(DateTime.Now);
        UpdateStatus();
    }

    // Setter methods with validation
    public void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("SA-GRD-001")
                .WithData("Field", nameof(studentId));
        }
        StudentId = studentId;
    }

    public void SetCourseId(Guid courseId)
    {
        if (courseId == Guid.Empty)
        {
            throw new BusinessException("SA-GRD-002")
                .WithData("Field", nameof(courseId));
        }
        CourseId = courseId;
    }

    public void SetGradeValue(double gradeValue)
    {
        if (gradeValue < GradeConsts.MinGradeValue || gradeValue > GradeConsts.MaxGradeValue)
        {
            throw new BusinessException("SA-GRD-003")
                .WithData("MinGrade", GradeConsts.MinGradeValue)
                .WithData("MaxGrade", GradeConsts.MaxGradeValue)
                .WithData("ProvidedGrade", gradeValue);
        }
        GradeValue = gradeValue;
        UpdateStatus();
    }

    public void SetComments(string? comments)
    {
        if (!string.IsNullOrWhiteSpace(comments))
        {
            Check.Length(comments, nameof(comments), GradeConsts.MaxCommentsLength);
        }
        Comments = comments;
    }

    public void SetGradedAt(DateTime? gradedAt)
    {
        GradedAt = gradedAt;
    }

    /// <summary>
    /// Updates grade status based on grade value
    /// </summary>
    private void UpdateStatus()
    {
        if (GradeValue >= GradeConsts.PassingGrade)
        {
            Status = EnumGradeStatus.Passed;
        }
        else if (GradeValue > 0)
        {
            Status = EnumGradeStatus.Failed;
        }
        else
        {
            Status = EnumGradeStatus.Incomplete;
        }
    }

    /// <summary>
    /// Marks grade as incomplete
    /// </summary>
    public void MarkAsIncomplete()
    {
        Status = EnumGradeStatus.Incomplete;
        GradeValue = 0;
    }
}
