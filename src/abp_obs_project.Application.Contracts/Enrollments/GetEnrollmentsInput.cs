using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Input for querying enrollments
/// </summary>
public class GetEnrollmentsInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public EnumEnrollmentStatus? Status { get; set; }
    public DateTime? EnrolledAtMin { get; set; }
    public DateTime? EnrolledAtMax { get; set; }

    public GetEnrollmentsInput()
    {
        Sorting = EnrollmentConsts.GetDefaultSorting();
    }
}
