using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using abp_obs_project.Courses;
using abp_obs_project.Students;
using abp_obs_project.Grades;
using abp_obs_project.Attendances;
using abp_obs_project.Permissions;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Blazor.Components.Pages.Student;

public partial class StudentDashboard
{
    [Inject] protected ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] protected IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] protected IGradeAppService GradeAppService { get; set; } = default!;
    [Inject] protected IAttendanceAppService AttendanceAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected int TotalEnrolledCourses { get; set; }
    protected double AverageGrade { get; set; }
    protected int TotalAbsences { get; set; }
    protected bool IsStudent { get; set; }
    protected List<GradeDto> RecentGrades { get; set; } = new();
    protected List<CourseDto> RecentCourses { get; set; } = new();
    protected Guid? CurrentStudentId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is authenticated
        if (CurrentUser.Id.HasValue)
        {
            IsStudent = await IsStudentAsync();

            if (IsStudent && CurrentStudentId.HasValue)
            {
                await LoadStatisticsAsync();
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

            // Try to find student record using AppService (self only)
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

    private async Task LoadStatisticsAsync()
    {
        try
        {
            if (!CurrentStudentId.HasValue)
            {
                return;
            }

            // Load grades for current student using AppService
            var myGradesResult = await GradeAppService.GetMyGradesAsync();
            var allGrades = myGradesResult.Items.ToList();

            // Calculate total enrolled courses (unique courses with grades)
            var enrolledCourseIds = allGrades.Select(g => g.CourseId).Distinct().ToList();
            TotalEnrolledCourses = enrolledCourseIds.Count;

            // Calculate average grade (GPA)
            // Only include grades with value > 0 to exclude enrollment-only records
            var validGrades = allGrades.Where(g => g.GradeValue > 0).ToList();
            if (validGrades.Any())
            {
                AverageGrade = Math.Round(validGrades.Average(g => g.GradeValue), 2);
            }
            else
            {
                AverageGrade = 0.0;
            }

            // Get recent grades (last 5 grades with value > 0)
            // We need to load course names, so we'll convert to DTOs
            var recentGradeEntities = allGrades
                .Where(g => g.GradeValue > 0)
                .OrderByDescending(g => g.CreationTime)
                .Take(5)
                .ToList();

            RecentGrades = new List<GradeDto>();
            foreach (var grade in recentGradeEntities)
            {
                RecentGrades.Add(new GradeDto
                {
                    Id = grade.Id,
                    StudentId = grade.StudentId,
                    CourseId = grade.CourseId,
                    CourseName = grade.CourseName,
                    GradeValue = grade.GradeValue,
                    Comments = grade.Comments
                });
            }

            // Load attendances for current student using AppService
            var attendancesResult = await AttendanceAppService.GetMyAttendancesAsync();
            var attendances = attendancesResult.Items.ToList();

            // Count absences (where IsPresent = false)
            TotalAbsences = attendances.Count(a => !a.IsPresent);

            // Load recent courses
            if (enrolledCourseIds.Any())
            {
                var myCourses = await CourseAppService.GetMyCoursesAsync();
                RecentCourses = myCourses.Items
                    .Where(c => enrolledCourseIds.Contains(c.Id))
                    .OrderByDescending(c => c.CreationTime)
                    .Take(5)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
