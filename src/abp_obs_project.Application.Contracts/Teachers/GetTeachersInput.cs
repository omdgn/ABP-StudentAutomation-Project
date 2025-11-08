using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Teachers;

/// <summary>
/// Input DTO for filtering and paging teacher list
/// </summary>
public class GetTeachersInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
}
