using System;
using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Grades;

/// <summary>
/// DTO for creating or updating a grade
/// </summary>
public class CreateUpdateGradeDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    [Range(GradeConsts.MinGradeValue, GradeConsts.MaxGradeValue)]
    public double GradeValue { get; set; }

    [StringLength(GradeConsts.MaxCommentsLength)]
    public string? Comments { get; set; }
}
