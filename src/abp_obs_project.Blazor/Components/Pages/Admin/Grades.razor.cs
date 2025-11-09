using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Permissions;
using abp_obs_project.Grades;
using abp_obs_project.Students;
using abp_obs_project.Courses;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Grades
{
    private IReadOnlyList<GradeDto> GradeList { get; set; } = new List<GradeDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;

    // Filters
    private Guid? FilterStudentId { get; set; }
    private Guid? FilterCourseId { get; set; }
    private EnumGradeStatus? FilterStatus { get; set; }

    // Lookup data
    private List<StudentDto> AllStudents { get; set; } = new();
    private List<CourseDto> AllCourses { get; set; } = new();

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
        await LoadLookupsAsync();
        // Ensure grid is populated on first render even if ReadData isn't triggered immediately
        CurrentPage = 1;
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

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<GradeDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");

        CurrentPage = e.Page;
        await GetGradesAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task GetGradesAsync()
    {
        try
        {
            var input = new GetGradesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting,
                StudentId = FilterStudentId,
                CourseId = FilterCourseId,
                Status = FilterStatus
            };

            var result = await GradeAppService.GetListAsync(input);
            GradeList = result.Items;
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
        await GetGradesAsync();
    }

    private async Task OnCourseFilterChanged(Guid? courseId)
    {
        FilterCourseId = courseId;
        CurrentPage = 1;
        await GetGradesAsync();
    }

    private async Task OnStatusFilterChanged(EnumGradeStatus? status)
    {
        FilterStatus = status;
        CurrentPage = 1;
        await GetGradesAsync();
    }

    // Create Modal
    private async Task OpenCreateModalAsync()
    {
        NewGrade = new CreateUpdateGradeDto
        {
            GradeValue = 0
        };
        await CreateModal.Show();
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
