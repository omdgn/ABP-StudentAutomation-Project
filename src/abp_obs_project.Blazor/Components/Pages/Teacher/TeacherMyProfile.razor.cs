using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Teachers;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherMyProfile
{
    [Inject] protected ITeacherAppService TeacherAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected bool IsTeacher { get; set; }
    protected TeacherDto? Me { get; set; }
    protected UpdateMyTeacherProfileDto EditModel { get; set; } = new();
    protected string? SaveSuccessMessage { get; set; }
    protected string? SaveErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Basic teacher check via default permission
        IsTeacher = await AuthorizationService.IsGrantedAsync(Permissions.abp_obs_projectPermissions.Teachers.Default);
        if (!IsTeacher)
        {
            return;
        }

        var me = await TeacherAppService.GetMeAsync();
        if (me != null)
        {
            Me = me;
            EditModel = new UpdateMyTeacherProfileDto
            {
                FirstName = me.FirstName,
                LastName = me.LastName,
                Department = me.Department,
                PhoneNumber = me.PhoneNumber
            };
        }
        else
        {
            IsTeacher = false;
        }
    }

    protected async Task SaveAsync()
    {
        try
        {
            var updated = await TeacherAppService.UpdateMyProfileAsync(EditModel);
            Me = updated;
            EditModel.FirstName = updated.FirstName;
            EditModel.LastName = updated.LastName;
            EditModel.Department = updated.Department;
            EditModel.PhoneNumber = updated.PhoneNumber;

            SaveErrorMessage = null;
            SaveSuccessMessage = L?["ProfileUpdated"] ?? "Profile updated successfully.";
        }
        catch (Exception ex)
        {
            SaveSuccessMessage = null;
            SaveErrorMessage = (L?["ProfileUpdateFailed"] ?? "Profile update failed.") + $" {ex.Message}";
        }

        await InvokeAsync(StateHasChanged);
    }
}

