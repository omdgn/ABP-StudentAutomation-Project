using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using abp_obs_project.Permissions;
using abp_obs_project.Attendances;
using abp_obs_project.Students;
using abp_obs_project.Courses;
using abp_obs_project.Enrollments;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherAttendances
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IEnrollmentAppService EnrollmentAppService { get; set; } = default!;

    private IReadOnlyList<AttendanceDto> AttendanceList { get; set; } = new List<AttendanceDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;

    // Filters
    private Guid? FilterCourseId { get; set; }
    private bool? FilterIsPresent { get; set; }
    private DateTime? FilterDate { get; set; }

    // Lookup data
    private List<StudentDto> AllStudents { get; set; } = new();
    private List<StudentDto> FilteredStudentsForForm { get; set; } = new();
    private List<CourseDto> MyCourses { get; set; } = new();

    // Permissions
    private bool CanCreateAttendance { get; set; }
    private bool CanEditAttendance { get; set; }
    private bool CanDeleteAttendance { get; set; }

    // Modal and DataGrid
    private Modal EditModal { get; set; } = null!;
    private DataGrid<AttendanceDto> AttendanceDataGrid { get; set; } = null!;

    // DTOs
    private CreateUpdateAttendanceDto NewAttendance { get; set; } = new();
    private CreateUpdateAttendanceDto EditingAttendance { get; set; } = new();
    private Guid EditingAttendanceId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SetPermissionsAsync();

        // Check for courseId query parameter
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("courseId", out var courseId))
        {
            if (Guid.TryParse(courseId, out var parsedCourseId))
            {
                FilterCourseId = parsedCourseId;
            }
        }

        await LoadLookupsAsync();

        // Initialize new attendance with defaults
        NewAttendance = new CreateUpdateAttendanceDto
        {
            AttendanceDate = DateTime.Now,
            IsPresent = true,
            CourseId = FilterCourseId ?? Guid.Empty
        };

        // Load students for the preselected course (if any)
        if (NewAttendance.CourseId != Guid.Empty)
        {
            await LoadStudentsForCourseAsync(NewAttendance.CourseId);
        }
        else
        {
            FilteredStudentsForForm = new List<StudentDto>();
        }
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
            // Load only teacher's courses
            var courseResult = await CourseAppService.GetListAsync(new GetCoursesInput { MaxResultCount = 1000 });
            MyCourses = courseResult.Items.ToList();

            // Load students
            var studentResult = await StudentAppService.GetListAsync(new GetStudentsInput { MaxResultCount = 1000 });
            AllStudents = studentResult.Items.ToList();
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
            // Clear the list first to force re-render
            AttendanceList = new List<AttendanceDto>();
            TotalCount = 0;
            await InvokeAsync(StateHasChanged);

            var input = new GetAttendancesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
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
            AttendanceList = result.Items.ToList();
            TotalCount = (int)result.TotalCount;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task LoadStudentsForCourseAsync(Guid courseId)
    {
        try
        {
            // Get enrollments for this course
            var enrollments = await EnrollmentAppService.GetListAsync(new GetEnrollmentsInput
            {
                CourseId = courseId,
                Status = EnumEnrollmentStatus.Active,
                MaxResultCount = 1000
            });

            // Get student IDs from enrollments
            var enrolledStudentIds = enrollments.Items.Select(e => e.StudentId).ToHashSet();

            // Filter students to only show enrolled ones
            FilteredStudentsForForm = AllStudents.Where(s => enrolledStudentIds.Contains(s.Id)).ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task OnCourseFilterChangedString(string courseIdString)
    {
        if (string.IsNullOrEmpty(courseIdString))
        {
            FilterCourseId = null;
        }
        else if (Guid.TryParse(courseIdString, out var courseId))
        {
            FilterCourseId = courseId;
        }

        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task OnStatusFilterChangedString(string isPresentString)
    {
        if (string.IsNullOrEmpty(isPresentString))
        {
            FilterIsPresent = null;
        }
        else if (bool.TryParse(isPresentString, out var isPresent))
        {
            FilterIsPresent = isPresent;
        }

        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task OnFormCourseChanged(Guid courseId)
    {
        NewAttendance.CourseId = courseId;
        NewAttendance.StudentId = Guid.Empty; // Reset student selection

        if (courseId != Guid.Empty)
        {
            await LoadStudentsForCourseAsync(courseId);
        }
        else
        {
            FilteredStudentsForForm = new List<StudentDto>();
        }

        StateHasChanged();
    }

    private async Task OnDateFilterChanged(DateTime? date)
    {
        FilterDate = date;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task ClearDateFilterAsync()
    {
        FilterDate = null;
        CurrentPage = 1;
        await GetAttendancesAsync();
    }

    private async Task ReloadDataGridAsync()
    {
        await AttendanceDataGrid.Reload();
        await InvokeAsync(StateHasChanged);
    }

    // Create Attendance
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
            await ReloadDataGridAsync();

            // Reset form
            NewAttendance = new CreateUpdateAttendanceDto
            {
                AttendanceDate = DateTime.Now,
                IsPresent = true,
                CourseId = FilterCourseId ?? Guid.Empty
            };

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
            await ReloadDataGridAsync();
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
            await ReloadDataGridAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
