using abp_obs_project.Blazor.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Teachers;

public partial class Index
{
    [Inject]
    private ITeacherUIService TeacherUIService { get; set; } = default!;

    // TODO: Implement teacher management
}
