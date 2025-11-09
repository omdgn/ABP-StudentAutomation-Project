using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Enrollment data transfer object with student and course information
/// </summary>
public class EnrollmentDto : AuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? WithdrawnAt { get; set; }
    public EnumEnrollmentStatus Status { get; set; }

    // Navigation properties for display purposes
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
}
