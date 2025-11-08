using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Permissions;
using abp_obs_project.Courses;
using abp_obs_project.Students;
using abp_obs_project.Teachers;

namespace abp_obs_project.Blazor.Components.Pages.Admin;

public partial class Dashboard
{
    protected int TotalStudents { get; set; }
    protected int TotalTeachers { get; set; }
    protected int TotalCourses { get; set; }
    protected int ActiveCourses { get; set; }
    protected bool IsAdmin { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is admin (has ViewAll permissions)
        IsAdmin = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll)
                  || await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);

        if (IsAdmin)
        {
            await LoadStatisticsAsync();
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Load total students
            if (await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll))
            {
                var studentsResult = await StudentAppService.GetListAsync(new GetStudentsInput { MaxResultCount = 1 });
                TotalStudents = (int)studentsResult.TotalCount;
            }

            // Load total teachers
            if (await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll))
            {
                var teachersResult = await TeacherAppService.GetListAsync(new GetTeachersInput { MaxResultCount = 1 });
                TotalTeachers = (int)teachersResult.TotalCount;
            }

            // Load total courses and active courses
            if (await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.ViewAll))
            {
                // Get total courses count
                var allCoursesResult = await CourseAppService.GetListAsync(new GetCoursesInput { MaxResultCount = 1 });
                TotalCourses = (int)allCoursesResult.TotalCount;

                // Get active courses count (InProgress status)
                var activeCoursesResult = await CourseAppService.GetListAsync(
                    new GetCoursesInput
                    {
                        Status = EnumCourseStatus.InProgress,
                        MaxResultCount = 1
                    });
                ActiveCourses = (int)activeCoursesResult.TotalCount;
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
