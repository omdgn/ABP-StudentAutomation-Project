using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Permissions;
using abp_obs_project.Attendances;
using abp_obs_project.Students;
using abp_obs_project.Courses;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Attendances
{
    private IReadOnlyList<AttendanceDto> AttendanceList { get; set; } = new List<AttendanceDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;

    // Filters
    private Guid? FilterStudentId { get; set; }
    private Guid? FilterCourseId { get; set; }
    private bool? FilterIsPresent { get; set; }
    private DateTime? FilterDate { get; set; }

    // Lookup data
    private List<StudentDto> AllStudents { get; set; } = new();
    private List<CourseDto> AllCourses { get; set; } = new();

    // Permissions
    private bool CanCreateAttendance { get; set; }
    private bool CanEditAttendance { get; set; }
    private bool CanDeleteAttendance { get; set; }

    // Modals
    private Modal CreateModal { get; set; } = null!;
    private Modal EditModal { get; set; } = null!;

    // DTOs
    private CreateUpdateAttendanceDto NewAttendance { get; set; } = new();
    private CreateUpdateAttendanceDto EditingAttendance { get; set; } = new();
    private Guid EditingAttendanceId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SetPermissionsAsync();
        await LoadLookupsAsync();
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateAttendance = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Create);
        CanEditAttendance = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Edit);
        CanDeleteAttendance = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Delete);
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            // Load students
            var studentResult = await StudentAppService.GetListAsync(new GetStudentsInput { MaxResultCount = 1000 });
            AllStudents = studentResult.Items.ToList();

            // Load courses
            var courseResult = await CourseAppService.GetListAsync(new GetCoursesInput { MaxResultCount = 1000 });
            AllCourses = courseResult.Items.ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<AttendanceDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");

        CurrentPage = e.Page;
        await GetAttendancesAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task GetAttendancesAsync()
    {
        try
        {
            var input = new GetAttendancesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
                StudentId = FilterStudentId,
                CourseId = FilterCourseId,
                IsPresent = FilterIsPresent
            };

            // Add date filter if specified
            if (FilterDate.HasValue)
            {
                input.AttendanceDateMin = FilterDate.Value.Date;
                input.AttendanceDateMax = FilterDate.Value.Date.AddDays(1).AddSeconds(-1);
            }

            var result = await AttendanceAppService.GetListAsync(input);
            AttendanceList = result.Items;
            TotalCount = (int)result.TotalCount;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task OnStudentFilterChanged(Guid? studentId)
    {
        FilterStudentId = studentId;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task OnCourseFilterChanged(Guid? courseId)
    {
        FilterCourseId = courseId;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task OnStatusFilterChanged(bool? isPresent)
    {
        FilterIsPresent = isPresent;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task ClearDateFilterAsync()
    {
        FilterDate = null;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewAttendance = new CreateUpdateAttendanceDto
        {
            AttendanceDate = DateTime.Now,
            IsPresent = true
        };
        await CreateModal.Show();
    }

    private async Task CloseCreateModalAsync()
    {
        await CreateModal.Hide();
    }

    private async Task CreateAttendanceAsync()
    {
        try
        {
            if (NewAttendance.StudentId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectStudent"]);
                return;
            }

            if (NewAttendance.CourseId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectCourse"]);
                return;
            }

            await AttendanceAppService.CreateAsync(NewAttendance);
            await GetAttendancesAsync();
            await CreateModal.Hide();
            await Message.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Edit Modal
    private async Task OpenEditModalAsync(AttendanceDto attendance)
    {
        try
        {
            EditingAttendanceId = attendance.Id;
            var attendanceDto = await AttendanceAppService.GetAsync(attendance.Id);

            EditingAttendance = new CreateUpdateAttendanceDto
            {
                StudentId = attendanceDto.StudentId,
                CourseId = attendanceDto.CourseId,
                AttendanceDate = attendanceDto.AttendanceDate,
                IsPresent = attendanceDto.IsPresent,
                Remarks = attendanceDto.Remarks
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

    private async Task UpdateAttendanceAsync()
    {
        try
        {
            if (EditingAttendance.StudentId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectStudent"]);
                return;
            }

            if (EditingAttendance.CourseId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectCourse"]);
                return;
            }

            await AttendanceAppService.UpdateAsync(EditingAttendanceId, EditingAttendance);
            await GetAttendancesAsync();
            await EditModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Delete
    private async Task DeleteAttendanceAsync(AttendanceDto attendance)
    {
        var confirmMessage = string.Format(L["AttendanceDeletionConfirmationMessage"], attendance.StudentName, attendance.CourseName, attendance.AttendanceDate.ToShortDateString());
        if (!await Message.Confirm(confirmMessage))
        {
            return;
        }

        try
        {
            await AttendanceAppService.DeleteAsync(attendance.Id);
            await GetAttendancesAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
