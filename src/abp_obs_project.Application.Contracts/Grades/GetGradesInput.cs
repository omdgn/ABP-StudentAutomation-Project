using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Grades;

/// <summary>
/// Input DTO for filtering and paging grade list
/// </summary>
public class GetGradesInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public double? GradeValueMin { get; set; }
    public double? GradeValueMax { get; set; }
    public EnumGradeStatus? Status { get; set; }
}
