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

public partial class StudentMyCourses
{
    [Inject] protected ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] protected IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] protected IGradeAppService GradeAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected bool IsStudent { get; set; }
    protected Guid? CurrentStudentId { get; set; }
    protected List<CourseDto> MyCourses { get; set; } = new();
    protected List<CourseDto> FilteredCourses { get; set; } = new();
    protected List<GradeDto> CourseGrades { get; set; } = new();
    protected int TotalCourses { get; set; }
    protected int ActiveCourses { get; set; }
    protected int TotalCredits { get; set; }
    protected double AverageGrade { get; set; }

    // Filters
    protected string? SearchText { get; set; }
    protected EnumCourseStatus? StatusFilter { get; set; }
    protected int? MinCredits { get; set; }
    protected int? MaxCredits { get; set; }

    // Paging / sorting
    protected int PageSize { get; set; } = 9;
    protected int CurrentPage { get; set; } = 1;
    protected int TotalCount { get; set; }
    protected string CurrentSorting { get; set; } = nameof(CourseDto.Code);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is authenticated
        if (CurrentUser.Id.HasValue)
        {
            IsStudent = await IsStudentAsync();

            if (IsStudent && CurrentStudentId.HasValue)
            {
                await LoadMyCoursesAsync();
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

    private async Task LoadMyCoursesAsync()
    {
        try
        {
            if (!CurrentStudentId.HasValue)
            {
                return;
            }

            // Load grades for current student to find enrolled courses (self-only endpoint)
            var gradesResult = await GradeAppService.GetMyGradesAsync();
            CourseGrades = gradesResult.Items.ToList();

            // Server-side filtered list of my courses
            var input = new GetMyCoursesInput
            {
                FilterText = SearchText,
                Status = StatusFilter,
                CreditsMin = MinCredits,
                CreditsMax = MaxCredits,
                Sorting = CurrentSorting,
                SkipCount = (CurrentPage - 1) * PageSize,
                MaxResultCount = PageSize
            };

            var coursesResult = await CourseAppService.GetMyCoursesAsync(input);
            MyCourses = coursesResult.Items.ToList();
            TotalCount = (int)coursesResult.TotalCount;
            // Reflect the current page on the UI list
            FilteredCourses = MyCourses.ToList();

            if (MyCourses.Any())
            {
                // Calculate statistics
                TotalCourses = MyCourses.Count;
                ActiveCourses = MyCourses.Count(c => c.Status == EnumCourseStatus.InProgress);
                TotalCredits = MyCourses.Sum(c => c.Credits);

                // Calculate average grade (only for courses with grades > 0)
                var validGrades = CourseGrades.Where(g => g.GradeValue > 0).ToList();
                if (validGrades.Any())
                {
                    AverageGrade = Math.Round(validGrades.Average(g => g.GradeValue), 2);
                }
                else
                {
                    AverageGrade = 0.0;
                }

                ApplyFilters();
            }
            else
            {
                MyCourses = new List<CourseDto>();
                FilteredCourses = new List<CourseDto>();
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<CourseDto> query = MyCourses;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(c =>
                (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.Code) && c.Code.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.TeacherName) && c.TeacherName.Contains(s, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (StatusFilter.HasValue)
        {
            query = query.Where(c => c.Status == StatusFilter.Value);
        }

        if (MinCredits.HasValue)
        {
            query = query.Where(c => c.Credits >= MinCredits.Value);
        }

        if (MaxCredits.HasValue)
        {
            query = query.Where(c => c.Credits <= MaxCredits.Value);
        }

        // Client-side filter reflects current page subset only.
        FilteredCourses = query.ToList();
    }

    protected async Task OnSearchChanged(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString();
        CurrentPage = 1;
        await LoadMyCoursesAsync();
    }

    protected async Task OnStatusChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrEmpty(val))
        {
            StatusFilter = null;
        }
        else if (Enum.TryParse<EnumCourseStatus>(val, out var parsed))
        {
            StatusFilter = parsed;
        }
        CurrentPage = 1;
        await LoadMyCoursesAsync();
    }

    protected async Task OnMinCreditsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var v)) MinCredits = v; else MinCredits = null;
        CurrentPage = 1;
        await LoadMyCoursesAsync();
    }

    protected async Task OnMaxCreditsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var v)) MaxCredits = v; else MaxCredits = null;
        CurrentPage = 1;
        await LoadMyCoursesAsync();
    }

    protected async Task GoToPageAsync(int page)
    {
        if (page < 1) page = 1;
        var maxPage = (int)Math.Ceiling((double)TotalCount / PageSize);
        if (page > maxPage) page = maxPage;
        CurrentPage = page;
        await LoadMyCoursesAsync();
    }
}
