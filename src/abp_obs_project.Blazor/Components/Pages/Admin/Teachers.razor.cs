using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Permissions;
using abp_obs_project.Teachers;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Teachers
{
    private IReadOnlyList<TeacherDto> TeacherList { get; set; } = new List<TeacherDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;
    private string Filter { get; set; } = string.Empty;

    // Permissions
    private bool CanCreateTeacher { get; set; }
    private bool CanEditTeacher { get; set; }
    private bool CanDeleteTeacher { get; set; }

    // Modals
    private Modal CreateModal { get; set; } = null!;
    private Modal EditModal { get; set; } = null!;

    // DTOs
    private CreateTeacherWithUserDto NewTeacher { get; set; } = new();
    private CreateUpdateTeacherDto EditingTeacher { get; set; } = new();
    private Guid EditingTeacherId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SetPermissionsAsync();
        // Ensure grid is populated on first render even if ReadData isn't triggered immediately
        CurrentPage = 1;
        await GetTeachersAsync();
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateTeacher = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.Create);
        CanEditTeacher = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.Edit);
        CanDeleteTeacher = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.Delete);
    }

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<TeacherDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");

        // Blazorise uses 1-based page indexing
        CurrentPage = e.Page;

        await GetTeachersAsync();

        await InvokeAsync(StateHasChanged);
    }

    private async Task GetTeachersAsync()
    {
        try
        {
            var input = new GetTeachersInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
                FilterText = Filter
            };

            // Debug: Log search filter
            Console.WriteLine($"[Teachers Search] FilterText: '{input.FilterText}', Page: {CurrentPage}");

            var result = await TeacherAppService.GetListAsync(input);
            TeacherList = result.Items;
            TotalCount = (int)result.TotalCount;

            // Debug: Console log
            Console.WriteLine($"[Teachers Result] Page: {CurrentPage}, Skip: {input.SkipCount}, MaxResult: {input.MaxResultCount}, TotalCount: {TotalCount}, ItemCount: {TeacherList.Count}");

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task OnSearchTextChanged(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await GetTeachersAsync();
    }

    private async Task SearchTeachersAsync()
    {
        CurrentPage = 1;
        await GetTeachersAsync();
    }

    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        CurrentPage = 1;
        await GetTeachersAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewTeacher = new CreateTeacherWithUserDto
        {
            UserName = string.Empty,
            Password = string.Empty
        };

        await CreateModal.Show();
    }

    private async Task CloseCreateModalAsync()
    {
        await CreateModal.Hide();
    }

    private async Task CreateTeacherAsync()
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(NewTeacher.FirstName))
            {
                await Message.Error(L["FirstNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTeacher.LastName))
            {
                await Message.Error(L["LastNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTeacher.Email))
            {
                await Message.Error(L["EmailIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTeacher.UserName))
            {
                await Message.Error(L["UserNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTeacher.Password))
            {
                await Message.Error(L["PasswordIsRequired"]);
                return;
            }

            // Create teacher with identity user account (both in single transaction)
            await TeacherAppService.CreateTeacherWithUserAsync(NewTeacher);
            await GetTeachersAsync();
            await CreateModal.Hide();
            await Message.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Edit Modal
    private async Task OpenEditModalAsync(TeacherDto teacher)
    {
        try
        {
            EditingTeacherId = teacher.Id;
            var teacherDto = await TeacherAppService.GetAsync(teacher.Id);

            EditingTeacher = new CreateUpdateTeacherDto
            {
                FirstName = teacherDto.FirstName,
                LastName = teacherDto.LastName,
                Email = teacherDto.Email,
                Department = teacherDto.Department,
                PhoneNumber = teacherDto.PhoneNumber
            };

            await EditModal.Show();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task CloseEditModalAsync()
    {
        await EditModal.Hide();
    }

    private async Task UpdateTeacherAsync()
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(EditingTeacher.FirstName))
            {
                await Message.Error(L["FirstNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingTeacher.LastName))
            {
                await Message.Error(L["LastNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingTeacher.Email))
            {
                await Message.Error(L["EmailIsRequired"]);
                return;
            }

            await TeacherAppService.UpdateAsync(EditingTeacherId, EditingTeacher);
            await GetTeachersAsync();
            await EditModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Delete
    private async Task DeleteTeacherAsync(TeacherDto teacher)
    {
        var confirmMessage = string.Format(L["TeacherDeletionConfirmationMessage"], teacher.FirstName, teacher.LastName);
        if (!await Message.Confirm(confirmMessage))
        {
            return;
        }

        try
        {
            await TeacherAppService.DeleteAsync(teacher.Id);
            await GetTeachersAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
