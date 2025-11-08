using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Grades;

/// <summary>
/// Grade data transfer object with student and course information
/// </summary>
public class GradeDto : AuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public double GradeValue { get; set; }
    public EnumGradeStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation properties for display purposes
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}
