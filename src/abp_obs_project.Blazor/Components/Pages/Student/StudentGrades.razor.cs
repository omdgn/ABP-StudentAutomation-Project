using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Courses;
using abp_obs_project.Students;
using abp_obs_project.Grades;
using abp_obs_project.Permissions;

namespace abp_obs_project.Blazor.Components.Pages.Student;

public partial class StudentGrades
{
    [Inject] protected ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] protected IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] protected IGradeAppService GradeAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected bool IsStudent { get; set; }
    protected Guid? CurrentStudentId { get; set; }
    protected List<GradeDto> AllGrades { get; set; } = new();
    protected List<GradeDto> FilteredGrades { get; set; } = new();
    protected List<CourseDto> AllCourses { get; set; } = new();
    protected string SelectedCourseFilter { get; set; } = string.Empty;

    // Statistics
    protected int TotalCourses { get; set; }
    protected double AverageGrade { get; set; }
    protected double HighestGrade { get; set; }
    protected double LowestGrade { get; set; }
    protected int ExcellentCount { get; set; }
    protected int GoodCount { get; set; }
    protected int SatisfactoryCount { get; set; }
    protected int FailCount { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is authenticated
        if (CurrentUser.Id.HasValue)
        {
            IsStudent = await IsStudentAsync();

            if (IsStudent && CurrentStudentId.HasValue)
            {
                await LoadGradesAsync();
            }
        }
        else
        {
            IsStudent = false;
        }
    }

    private async Task<bool> IsStudentAsync()
    {
        try
        {
            // Check if user is authenticated
            if (!CurrentUser.IsAuthenticated || string.IsNullOrEmpty(CurrentUser.Email))
            {
                return false;
            }

            // Get current student's own record without requiring ViewAll
            var student = await StudentAppService.GetMeAsync();

            if (student != null)
            {
                CurrentStudentId = student.Id;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
            return false;
        }
    }

    private async Task LoadGradesAsync()
    {
        try
        {
            if (!CurrentStudentId.HasValue)
            {
                return;
            }

            // Load all grades for current student (self endpoint)
            var gradesResult = await GradeAppService.GetMyGradesAsync();
            AllGrades = gradesResult.Items.ToList();
            FilteredGrades = AllGrades;

            // Load courses
            var coursesResult = await CourseAppService.GetMyCoursesAsync(new GetMyCoursesInput
            {
                MaxResultCount = 1000
            });
            AllCourses = coursesResult.Items.OrderBy(c => c.Code).ToList();

            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private void CalculateStatistics()
    {
        // Only consider grades with value > 0
        var validGrades = AllGrades.Where(g => g.GradeValue > 0).ToList();

        TotalCourses = AllGrades.Select(g => g.CourseId).Distinct().Count();

        if (validGrades.Any())
        {
            AverageGrade = Math.Round(validGrades.Average(g => g.GradeValue), 2);
            HighestGrade = validGrades.Max(g => g.GradeValue);
            LowestGrade = validGrades.Min(g => g.GradeValue);

            // Grade distribution
            ExcellentCount = validGrades.Count(g => g.GradeValue >= 85);
            GoodCount = validGrades.Count(g => g.GradeValue >= 70 && g.GradeValue < 85);
            SatisfactoryCount = validGrades.Count(g => g.GradeValue >= 60 && g.GradeValue < 70);
            FailCount = validGrades.Count(g => g.GradeValue < 60);
        }
        else
        {
            AverageGrade = 0.0;
            HighestGrade = 0.0;
            LowestGrade = 0.0;
            ExcellentCount = 0;
            GoodCount = 0;
            SatisfactoryCount = 0;
            FailCount = 0;
        }
    }

    private async Task OnCourseFilterChanged(string courseId)
    {
        SelectedCourseFilter = courseId;

        if (string.IsNullOrEmpty(courseId))
        {
            FilteredGrades = AllGrades;
        }
        else
        {
            if (Guid.TryParse(courseId, out var guidCourseId))
            {
                FilteredGrades = AllGrades.Where(g => g.CourseId == guidCourseId).ToList();
            }
        }

        await InvokeAsync(StateHasChanged);
    }
}
