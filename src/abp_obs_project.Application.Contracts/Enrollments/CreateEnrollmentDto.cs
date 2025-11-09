using System;
using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Enrollments;

/// <summary>
/// DTO for creating a new enrollment
/// </summary>
public class CreateEnrollmentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}
