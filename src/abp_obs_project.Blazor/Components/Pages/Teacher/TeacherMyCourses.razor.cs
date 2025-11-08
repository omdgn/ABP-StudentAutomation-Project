using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.Permissions;
using abp_obs_project.Teachers;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherMyCourses
{
    [Inject] public ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected List<CourseDto> MyCourses { get; set; } = new();
    protected bool IsTeacher { get; set; }
    protected Guid? CurrentTeacherId { get; set; }
    
    // Modal
    private Modal CourseDetailsModal { get; set; } = null!;
    private CourseDto? SelectedCourse { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is a teacher
        IsTeacher = await IsTeacherAsync();

        if (IsTeacher)
        {
            await LoadMyCoursesAsync();
        }
    }

    private async Task<bool> IsTeacherAsync()
    {
        // Teacher should have Courses.Default permission but NOT ViewAll
        var hasCoursePermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default);
        var hasViewAllPermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.ViewAll);

        // If user is authenticated and has Course permission but NOT ViewAll, they are teacher
        return CurrentUser.IsAuthenticated && hasCoursePermission && !hasViewAllPermission;
    }

    private async Task LoadMyCoursesAsync()
    {
        try
        {
            // Service enforces teacher scoping by current user email when user lacks ViewAll
            var result = await CourseAppService.GetListAsync(new GetCoursesInput
            {
                MaxResultCount = 1000
            });

            MyCourses = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
    
    private async Task OpenCourseDetailsModal(CourseDto course)
    {
        SelectedCourse = course;
        await CourseDetailsModal.Show();
    }
    
    private async Task CloseCourseDetailsModal()
    {
        await CourseDetailsModal.Hide();
        SelectedCourse = null;
    }
}
