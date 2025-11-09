using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Students;

namespace abp_obs_project.Blazor.Components.Pages.Student;

public partial class StudentProfile
{
    [Inject] protected IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected bool IsStudent { get; set; }
    protected StudentDto? Me { get; set; }
    protected UpdateMyProfileDto EditModel { get; set; } = new();
    protected string? SaveSuccessMessage { get; set; }
    protected string? SaveErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (CurrentUser.Id.HasValue)
        {
            var me = await StudentAppService.GetMeAsync();
            if (me != null)
            {
                IsStudent = true;
                Me = me;
                EditModel = new UpdateMyProfileDto
                {
                    FirstName = me.FirstName,
                    LastName = me.LastName,
                    BirthDate = me.BirthDate,
                    Gender = me.Gender,
                    Phone = me.Phone
                };
            }
            else
            {
                IsStudent = false;
            }
        }
        else
        {
            IsStudent = false;
        }
    }

    protected async Task SaveAsync()
    {
        if (!IsStudent || Me == null)
        {
            return;
        }
        try
        {
            var updated = await StudentAppService.UpdateMyProfileAsync(EditModel);
            Me = updated;
            // Keep EditModel in sync
            EditModel.FirstName = updated.FirstName;
            EditModel.LastName = updated.LastName;
            EditModel.BirthDate = updated.BirthDate;
            EditModel.Gender = updated.Gender;
            EditModel.Phone = updated.Phone;

            SaveErrorMessage = null;
            SaveSuccessMessage = L?["ProfileUpdated"] ?? "Profile updated successfully.";
        }
        catch (Exception ex)
        {
            SaveSuccessMessage = null;
            SaveErrorMessage = (L?["ProfileUpdateFailed"] ?? "Profile update failed.") + $" {ex.Message}";
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
