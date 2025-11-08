using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Attendances;

/// <summary>
/// Input DTO for filtering and paging attendance list
/// Supports date range filtering for reports
/// </summary>
public class GetAttendancesInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public DateTime? AttendanceDateMin { get; set; }
    public DateTime? AttendanceDateMax { get; set; }
    public bool? IsPresent { get; set; }
}
