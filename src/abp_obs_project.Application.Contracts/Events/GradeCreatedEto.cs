using System;
using abp_obs_project.Grades;

namespace abp_obs_project.Events;

/// <summary>
/// Event Transfer Object for Grade Creation
/// Published when a new grade is assigned
/// Triggers: Email notification, grade statistics update, etc.
/// </summary>
[Serializable]
public class GradeCreatedEto
{
    public Guid GradeId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public double GradeValue { get; set; }
    public EnumGradeStatus Status { get; set; }
    public DateTime GradedAt { get; set; }

    // Additional info for handlers (denormalized for performance)
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}
