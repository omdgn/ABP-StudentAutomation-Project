using abp_obs_project.Blazor.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Grades;

public partial class Index
{
    [Inject]
    private IGradeUIService GradeUIService { get; set; } = default!;

    // TODO: Implement grade management
}
