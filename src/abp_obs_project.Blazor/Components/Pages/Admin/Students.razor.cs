using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Permissions;
using abp_obs_project.Students;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Students
{
    private IReadOnlyList<StudentDto> StudentList { get; set; } = new List<StudentDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;
    private string Filter { get; set; } = string.Empty;

    // Permissions
    private bool CanCreateStudent { get; set; }
    private bool CanEditStudent { get; set; }
    private bool CanDeleteStudent { get; set; }

    // Modals
    private Modal CreateModal { get; set; } = null!;
    private Modal EditModal { get; set; } = null!;

    // DTOs
    private CreateStudentWithUserDto NewStudent { get; set; } = new();
    private CreateUpdateStudentDto EditingStudent { get; set; } = new();
    private Guid EditingStudentId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SetPermissionsAsync();
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateStudent = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.Create);
        CanEditStudent = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.Edit);
        CanDeleteStudent = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.Delete);
    }

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<StudentDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");

        // Blazorise uses 1-based page indexing
        CurrentPage = e.Page;

        await GetStudentsAsync();

        await InvokeAsync(StateHasChanged);
    }

    private async Task GetStudentsAsync()
    {
        try
        {
            var input = new GetStudentsInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
                FilterText = Filter
            };

            var result = await StudentAppService.GetListAsync(input);
            StudentList = result.Items;
            TotalCount = (int)result.TotalCount;

            // Debug: Console log
            Console.WriteLine($"Page: {CurrentPage}, Skip: {input.SkipCount}, MaxResult: {input.MaxResultCount}, TotalCount: {TotalCount}, ItemCount: {StudentList.Count}");
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
        await GetStudentsAsync();
    }

    private async Task SearchStudentsAsync()
    {
        CurrentPage = 1;
        await GetStudentsAsync();
    }

    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        CurrentPage = 1;
        await GetStudentsAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewStudent = new CreateStudentWithUserDto
        {
            BirthDate = DateTime.Now.AddYears(-18),
            EnrollmentDate = DateTime.Now,
            Gender = EnumGender.Unknown,
            UserName = string.Empty,
            Password = string.Empty
        };

        await CreateModal.Show();
    }

    private async Task CloseCreateModalAsync()
    {
        await CreateModal.Hide();
    }

    private async Task CreateStudentAsync()
    {
        try
        {
            // Validate username and password
            if (string.IsNullOrWhiteSpace(NewStudent.UserName))
            {
                await Message.Error(L["UserNameIsRequired"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewStudent.Password))
            {
                await Message.Error(L["PasswordIsRequired"]);
                return;
            }

            // Create student with identity user account
            await StudentAppService.CreateStudentWithUserAsync(NewStudent);
            await GetStudentsAsync();
            await CreateModal.Hide();
            await Message.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Edit Modal
    private async Task OpenEditModalAsync(StudentDto student)
    {
        try
        {
            EditingStudentId = student.Id;
            var studentDto = await StudentAppService.GetAsync(student.Id);

            EditingStudent = new CreateUpdateStudentDto
            {
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                Email = studentDto.Email,
                StudentNumber = studentDto.StudentNumber,
                Gender = studentDto.Gender,
                BirthDate = studentDto.BirthDate,
                Phone = studentDto.Phone,
                EnrollmentDate = studentDto.EnrollmentDate,
                TeacherId = studentDto.TeacherId
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

    private async Task UpdateStudentAsync()
    {
        try
        {
            await StudentAppService.UpdateAsync(EditingStudentId, EditingStudent);
            await GetStudentsAsync();
            await EditModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Delete
    private async Task DeleteStudentAsync(StudentDto student)
    {
        var confirmMessage = string.Format(L["StudentDeletionConfirmationMessage"], student.FirstName, student.LastName);
        if (!await Message.Confirm(confirmMessage))
        {
            return;
        }

        try
        {
            await StudentAppService.DeleteAsync(student.Id);
            await GetStudentsAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
