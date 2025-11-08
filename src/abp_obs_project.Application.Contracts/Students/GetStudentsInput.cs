using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Students;

public class GetStudentsInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? StudentNumber { get; set; }
    public EnumGender? Gender { get; set; }
    public DateTime? BirthDateMin { get; set; }
    public DateTime? BirthDateMax { get; set; }

    public GetStudentsInput()
    {
        Sorting = StudentConsts.GetDefaultSorting(false);
    }
}
