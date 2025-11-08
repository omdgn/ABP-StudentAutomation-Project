using abp_obs_project.Blazor.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Attendances;

public partial class Index
{
    [Inject]
    private IAttendanceUIService AttendanceUIService { get; set; } = default!;

    // TODO: Implement attendance management
}
