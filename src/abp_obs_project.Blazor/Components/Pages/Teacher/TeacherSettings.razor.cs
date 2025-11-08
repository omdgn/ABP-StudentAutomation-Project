using System.Threading.Tasks;
using abp_obs_project.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Volo.Abp.Users;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherSettings
{
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject] public new ICurrentUser CurrentUser { get; set; } = default!;

    protected bool IsTeacher { get; set; }
    protected string? CurrentUserEmail { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is a teacher
        IsTeacher = await IsTeacherAsync();

        if (IsTeacher)
        {
            CurrentUserEmail = CurrentUser.Email;
        }
    }

    private async Task<bool> IsTeacherAsync()
    {
        // Teacher should have Course/Grade permission but NOT ViewAll permissions
        var hasCoursePermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default);
        var hasStudentViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);

        // If user is authenticated and has permission but NOT ViewAll, they are teacher
        return CurrentUser.IsAuthenticated && hasCoursePermission && !hasStudentViewAll;
    }
}
