using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Courses;

/// <summary>
/// Filter and paging input for student's own courses list.
/// Mirrors common list input patterns used in Teacher pages.
/// </summary>
public class GetMyCoursesInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public EnumCourseStatus? Status { get; set; }
    public int? CreditsMin { get; set; }
    public int? CreditsMax { get; set; }
}

