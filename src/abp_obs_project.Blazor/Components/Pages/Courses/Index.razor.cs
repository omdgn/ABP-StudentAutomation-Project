using abp_obs_project.Blazor.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Courses;

public partial class Index
{
    [Inject]
    private ICourseUIService CourseUIService { get; set; } = default!;

    // TODO: Implement course management
}
