using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Courses;

/// <summary>
/// Input DTO for filtering and paging course list
/// </summary>
public class GetCoursesInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? CreditsMin { get; set; }
    public int? CreditsMax { get; set; }
    public EnumCourseStatus? Status { get; set; }
    public Guid? TeacherId { get; set; }
}
