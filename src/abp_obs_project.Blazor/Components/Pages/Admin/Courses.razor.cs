using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Permissions;
using abp_obs_project.Courses;
using abp_obs_project.Teachers;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Courses
{
    private IReadOnlyList<CourseDto> CourseList { get; set; } = new List<CourseDto>();
    private IReadOnlyList<TeacherLookupDto> TeacherList { get; set; } = new List<TeacherLookupDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;
    private string Filter { get; set; } = string.Empty;

    // Permissions
    private bool CanCreateCourse { get; set; }
    private bool CanEditCourse { get; set; }
    private bool CanDeleteCourse { get; set; }

    // Modals
    private Modal CreateModal { get; set; } = null!;
    private Modal EditModal { get; set; } = null!;

    // DTOs
    private CreateUpdateCourseDto NewCourse { get; set; } = new();
    private CreateUpdateCourseDto EditingCourse { get; set; } = new();
    private Guid EditingCourseId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SetPermissionsAsync();
        await LoadTeachersAsync();
        // Ensure grid is populated on first render even if ReadData isn't triggered immediately
        CurrentPage = 1;
        await GetCoursesAsync();
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateCourse = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Create);
        CanEditCourse = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Edit);
        CanDeleteCourse = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Delete);
    }

    private async Task LoadTeachersAsync()
    {
        try
        {
            var teachers = await TeacherAppService.GetTeacherLookupAsync();
            TeacherList = teachers;
        }
        catch
        {
            // User doesn't have permission to load teachers - that's okay
            TeacherList = new List<TeacherLookupDto>();
        }
    }

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<CourseDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");

        // Blazorise uses 1-based page indexing
        CurrentPage = e.Page;

        await GetCoursesAsync();

        await InvokeAsync(StateHasChanged);
    }

    private async Task GetCoursesAsync()
    {
        try
        {
            var input = new GetCoursesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
                FilterText = Filter
            };

            var result = await CourseAppService.GetListAsync(input);
            CourseList = result.Items;
            TotalCount = (int)result.TotalCount;

            Console.WriteLine($"[Courses] Page: {CurrentPage}, Skip: {input.SkipCount}, MaxResult: {input.MaxResultCount}, TotalCount: {TotalCount}, ItemCount: {CourseList.Count}");

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
        await GetCoursesAsync();
    }

    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        CurrentPage = 1;
        await GetCoursesAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewCourse = new CreateUpdateCourseDto
        {
            Status = EnumCourseStatus.NotStarted,
            Credits = 3
        };

        await CreateModal.Show();
    }

    private async Task CloseCreateModalAsync()
    {
        await CreateModal.Hide();
    }

    private async Task CreateCourseAsync()
    {
        try
        {
            await CourseAppService.CreateAsync(NewCourse);
            await GetCoursesAsync();
            await CreateModal.Hide();
            await Message.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Edit Modal
    private async Task OpenEditModalAsync(CourseDto course)
    {
        try
        {
            EditingCourseId = course.Id;
            var courseDto = await CourseAppService.GetAsync(course.Id);

            EditingCourse = new CreateUpdateCourseDto
            {
                Name = courseDto.Name,
                Code = courseDto.Code,
                Credits = courseDto.Credits,
                Description = courseDto.Description,
                Status = courseDto.Status,
                TeacherId = courseDto.TeacherId
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

    private async Task UpdateCourseAsync()
    {
        try
        {
            await CourseAppService.UpdateAsync(EditingCourseId, EditingCourse);
            await GetCoursesAsync();
            await EditModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Delete
    private async Task DeleteCourseAsync(CourseDto course)
    {
        var confirmMessage = string.Format(L["CourseDeletionConfirmationMessage"], course.Name);
        if (!await Message.Confirm(confirmMessage))
        {
            return;
        }

        try
        {
            await CourseAppService.DeleteAsync(course.Id);
            await GetCoursesAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
