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

        // Check if user is a student
        IsStudent = await IsStudentAsync();

        if (IsStudent && CurrentStudentId.HasValue)
        {
            await LoadStatisticsAsync();
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

            // Check if user has admin or teacher permissions (if yes, not a student)
            var hasStudentsViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
            var hasTeachersViewAll = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);
            var hasGradesCreate = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Create);
            var hasAttendancesCreate = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Create);
            var hasCoursesCreate = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Create);

            // If user has admin or teacher permissions, they are NOT a student
            if (hasStudentsViewAll || hasTeachersViewAll || hasGradesCreate || hasAttendancesCreate || hasCoursesCreate)
            {
                return false;
            }

            // Try to find student record by email
            var studentsResult = await StudentAppService.GetListAsync(new GetStudentsInput
            {
                MaxResultCount = 1000
            });

            var student = studentsResult.Items.FirstOrDefault(s =>
                s.Email != null &&
                s.Email.Equals(CurrentUser.Email, StringComparison.OrdinalIgnoreCase));

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

            // Load grades for current student
            var gradesResult = await GradeAppService.GetListAsync(new GetGradesInput
            {
                StudentId = CurrentStudentId.Value,
                MaxResultCount = 1000
            });

            var allGrades = gradesResult.Items.ToList();

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
            RecentGrades = allGrades
                .Where(g => g.GradeValue > 0)
                .OrderByDescending(g => g.CreationTime)
                .Take(5)
                .ToList();

            // Load attendances for current student
            var attendancesResult = await AttendanceAppService.GetListAsync(new GetAttendancesInput
            {
                StudentId = CurrentStudentId.Value,
                MaxResultCount = 1000
            });

            // Count absences (where IsPresent = false)
            TotalAbsences = attendancesResult.Items.Count(a => !a.IsPresent);

            // Load recent courses
            if (enrolledCourseIds.Any())
            {
                var coursesResult = await CourseAppService.GetListAsync(new GetCoursesInput
                {
                    MaxResultCount = 1000
                });

                RecentCourses = coursesResult.Items
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
