using System;
using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Courses;

/// <summary>
/// DTO for creating or updating a course
/// </summary>
public class CreateUpdateCourseDto
{
    [Required]
    [StringLength(CourseConsts.MaxNameLength, MinimumLength = CourseConsts.MinNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(CourseConsts.MaxCodeLength, MinimumLength = CourseConsts.MinCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(CourseConsts.MinCredits, CourseConsts.MaxCredits)]
    public int Credits { get; set; }

    [StringLength(CourseConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [Required]
    public EnumCourseStatus Status { get; set; }

    [Required]
    public Guid TeacherId { get; set; }
}
