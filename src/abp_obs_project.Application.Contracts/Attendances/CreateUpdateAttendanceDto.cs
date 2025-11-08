using System;
using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Attendances;

/// <summary>
/// DTO for creating or updating an attendance record
/// </summary>
public class CreateUpdateAttendanceDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    [Required]
    public bool IsPresent { get; set; }

    [StringLength(AttendanceConsts.MaxRemarksLength)]
    public string? Remarks { get; set; }
}
