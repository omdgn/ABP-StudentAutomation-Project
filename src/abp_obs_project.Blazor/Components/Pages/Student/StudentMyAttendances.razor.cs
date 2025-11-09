using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Attendances;
using abp_obs_project.Courses;
using abp_obs_project.Grades;
using abp_obs_project.Students;

namespace abp_obs_project.Blazor.Components.Pages.Student;

public partial class StudentMyAttendances
{
    [Inject] protected IAttendanceAppService AttendanceAppService { get; set; } = default!;
    [Inject] protected ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] protected IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected bool IsStudent { get; set; }
    protected Guid? CurrentStudentId { get; set; }

    protected List<AttendanceDto> AllAttendances { get; set; } = new();
    protected List<AttendanceDto> FilteredAttendances { get; set; } = new();
    protected List<CourseDto> MyCourses { get; set; } = new();

    // Filters
    protected Guid? CourseFilter { get; set; }
    protected bool? StatusFilter { get; set; }
    protected DateTime? DateFilter { get; set; }
    protected string? DateFilterString => DateFilter?.ToString("yyyy-MM-dd");

    // Stats
    protected int TotalCount { get; set; }
    protected int PresentCount { get; set; }
    protected int AbsentCount { get; set; }
    protected double AttendanceRate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (CurrentUser.Id.HasValue)
        {
            IsStudent = await IsStudentAsync();
            if (IsStudent && CurrentStudentId.HasValue)
            {
                await LoadAsync();
            }
        }
        else
        {
            IsStudent = false;
        }
    }

    private async Task<bool> IsStudentAsync()
    {
        var me = await StudentAppService.GetMeAsync();
        if (me != null)
        {
            CurrentStudentId = me.Id;
            return true;
        }
        return false;
    }

    private async Task LoadAsync()
    {
        // Load attendances & courses via self endpoints
        var attendances = await AttendanceAppService.GetMyAttendancesAsync();
        AllAttendances = attendances.Items.ToList();
        FilteredAttendances = AllAttendances.ToList();

        var courses = await CourseAppService.GetMyCoursesAsync(new GetMyCoursesInput { MaxResultCount = 1000 });
        MyCourses = courses.Items.OrderBy(c => c.Code).ToList();

        ApplyFilters();
        CalculateStats();
    }

    private void ApplyFilters()
    {
        IEnumerable<AttendanceDto> q = AllAttendances;
        if (CourseFilter.HasValue && CourseFilter != Guid.Empty)
        {
            q = q.Where(x => x.CourseId == CourseFilter.Value);
        }

        if (StatusFilter.HasValue)
        {
            q = q.Where(x => x.IsPresent == StatusFilter.Value);
        }

        if (DateFilter.HasValue)
        {
            var d = DateFilter.Value.Date;
            q = q.Where(x => x.AttendanceDate.Date == d);
        }

        FilteredAttendances = q.ToList();
    }

    private void CalculateStats()
    {
        TotalCount = FilteredAttendances.Count;
        PresentCount = FilteredAttendances.Count(x => x.IsPresent);
        AbsentCount = FilteredAttendances.Count(x => !x.IsPresent);
        AttendanceRate = TotalCount == 0 ? 0 : (double)PresentCount / TotalCount;
    }

    protected async Task OnCourseChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var id)) CourseFilter = id; else CourseFilter = null;
        ApplyFilters();
        CalculateStats();
        await InvokeAsync(StateHasChanged);
    }

    protected async Task OnStatusChanged(ChangeEventArgs e)
    {
        var v = e.Value?.ToString();
        if (string.IsNullOrEmpty(v)) StatusFilter = null;
        else if (bool.TryParse(v, out var b)) StatusFilter = b; else StatusFilter = null;
        ApplyFilters();
        CalculateStats();
        await InvokeAsync(StateHasChanged);
    }

    protected async Task OnDateChanged(ChangeEventArgs e)
    {
        var v = e.Value?.ToString();
        if (DateTime.TryParse(v, out var d)) DateFilter = d; else DateFilter = null;
        ApplyFilters();
        CalculateStats();
        await InvokeAsync(StateHasChanged);
    }

    protected async Task ClearFilters()
    {
        CourseFilter = null;
        StatusFilter = null;
        DateFilter = null;
        ApplyFilters();
        CalculateStats();
        await InvokeAsync(StateHasChanged);
    }
}

