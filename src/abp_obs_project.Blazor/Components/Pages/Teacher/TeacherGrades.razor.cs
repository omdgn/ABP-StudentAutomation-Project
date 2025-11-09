using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using abp_obs_project.Permissions;
using abp_obs_project.Grades;
using abp_obs_project.Students;
using abp_obs_project.Courses;
using abp_obs_project.Enrollments;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherGrades
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<GradeDto> GradeList { get; set; } = new List<GradeDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;

    // Filters
    private Guid? FilterStudentId { get; set; }
    private Guid? FilterCourseId { get; set; }

    // Lookup data
    private List<StudentDto> AllStudents { get; set; } = new();
    private List<StudentDto> FilteredStudents { get; set; } = new();
    private List<CourseDto> MyCourses { get; set; } = new();

    // Permissions
    private bool CanCreateGrade { get; set; }
    private bool CanEditGrade { get; set; }
    private bool CanDeleteGrade { get; set; }

    // Modals
    private Modal CreateModal { get; set; } = null!;
    private Modal EditModal { get; set; } = null!;

    // DTOs
    private CreateUpdateGradeDto NewGrade { get; set; } = new();
    private CreateUpdateGradeDto EditingGrade { get; set; } = new();
    private Guid EditingGradeId { get; set; }

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

        // Load initial data
        await GetGradesAsync();
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateGrade = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Create);
        CanEditGrade = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Edit);
        CanDeleteGrade = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Delete);
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

    private async Task GetGradesAsync()
    {
        try
        {
            // Clear the list first to force re-render
            GradeList = new List<GradeDto>();
            TotalCount = 0;
            await InvokeAsync(StateHasChanged);

            var input = new GetGradesInput
            {
                MaxResultCount = 1000, // Get all grades, no paging
                SkipCount = 0,
                StudentId = FilterStudentId,
                CourseId = FilterCourseId
            };

            Logger.LogInformation($"Getting grades - CourseId: {FilterCourseId}, StudentId: {FilterStudentId}");

            var result = await GradeAppService.GetListAsync(input);
            GradeList = result.Items.ToList(); // Create new list instance
            TotalCount = (int)result.TotalCount;

            Logger.LogInformation($"Got {GradeList.Count} grades");

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private string GetCourseFilterValue()
    {
        return FilterCourseId?.ToString() ?? string.Empty;
    }

    private string GetStudentFilterValue()
    {
        return FilterStudentId?.ToString() ?? string.Empty;
    }

    private async Task OnCourseFilterChangedString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            FilterCourseId = null;
        }
        else if (Guid.TryParse(value, out var guid))
        {
            FilterCourseId = guid;
        }

        await GetGradesAsync();
    }

    private async Task OnStudentFilterChangedString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            FilterStudentId = null;
        }
        else if (Guid.TryParse(value, out var guid))
        {
            FilterStudentId = guid;
        }

        await GetGradesAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewGrade = new CreateUpdateGradeDto
        {
            GradeValue = 0,
            CourseId = FilterCourseId ?? Guid.Empty
        };

        // Load students for the preselected course (if any)
        if (NewGrade.CourseId != Guid.Empty)
        {
            await LoadStudentsForCourseAsync(NewGrade.CourseId);
        }
        else
        {
            FilteredStudents = new List<StudentDto>();
        }

        await CreateModal.Show();
    }

    private async Task OnNewGradeCourseChanged(Guid courseId)
    {
        NewGrade.CourseId = courseId;
        NewGrade.StudentId = Guid.Empty; // Reset student selection

        if (courseId != Guid.Empty)
        {
            await LoadStudentsForCourseAsync(courseId);
        }
        else
        {
            FilteredStudents = new List<StudentDto>();
        }

        StateHasChanged();
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
            FilteredStudents = AllStudents.Where(s => enrolledStudentIds.Contains(s.Id)).ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task CloseCreateModalAsync()
    {
        await CreateModal.Hide();
    }

    private async Task CreateGradeAsync()
    {
        try
        {
            if (NewGrade.StudentId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectStudent"]);
                return;
            }

            if (NewGrade.CourseId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectCourse"]);
                return;
            }

            await GradeAppService.CreateAsync(NewGrade);
            await GetGradesAsync();
            await CreateModal.Hide();
            await Message.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Edit Modal
    private async Task OpenEditModalAsync(GradeDto grade)
    {
        try
        {
            EditingGradeId = grade.Id;
            var gradeDto = await GradeAppService.GetAsync(grade.Id);

            EditingGrade = new CreateUpdateGradeDto
            {
                StudentId = gradeDto.StudentId,
                CourseId = gradeDto.CourseId,
                GradeValue = gradeDto.GradeValue,
                Comments = gradeDto.Comments
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

    private async Task UpdateGradeAsync()
    {
        try
        {
            if (EditingGrade.StudentId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectStudent"]);
                return;
            }

            if (EditingGrade.CourseId == Guid.Empty)
            {
                await Message.Error(L["PleaseSelectCourse"]);
                return;
            }

            await GradeAppService.UpdateAsync(EditingGradeId, EditingGrade);
            await GetGradesAsync();
            await EditModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Delete
    private async Task DeleteGradeAsync(GradeDto grade)
    {
        var confirmMessage = string.Format(L["GradeDeletionConfirmationMessage"], grade.StudentName, grade.CourseName);
        if (!await Message.Confirm(confirmMessage))
        {
            return;
        }

        try
        {
            await GradeAppService.DeleteAsync(grade.Id);
            await GetGradesAsync();
            await Message.Success(L["SuccessfullyDeleted"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
