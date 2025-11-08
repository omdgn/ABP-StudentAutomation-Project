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
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    protected List<CourseDto> MyCourses { get; set; } = new();
    protected bool IsTeacher { get; set; }
    protected Guid? CurrentTeacherId { get; set; }

    // Modals
    private Modal CourseDetailsModal { get; set; } = null!;
    private Modal UpdateStatusModal { get; set; } = null!;

    // Selected Course
    private CourseDto? SelectedCourse { get; set; }
    private CourseDto? EditingCourse { get; set; }
    private EnumCourseStatus NewStatus { get; set; }

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

    // Course Details Modal
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

    // Update Status Modal
    private async Task OpenUpdateStatusModal(CourseDto course)
    {
        EditingCourse = course;
        NewStatus = course.Status;
        await UpdateStatusModal.Show();
    }

    private async Task CloseUpdateStatusModal()
    {
        await UpdateStatusModal.Hide();
        EditingCourse = null;
    }

    private async Task UpdateCourseStatusAsync()
    {
        if (EditingCourse == null) return;

        try
        {
            var updateDto = new CreateUpdateCourseDto
            {
                Name = EditingCourse.Name,
                Code = EditingCourse.Code,
                Credits = EditingCourse.Credits,
                Description = EditingCourse.Description,
                // EditingCourse.TeacherId is non-nullable Guid in CourseDto
                // so no need for null-coalescing; keep the existing teacher assignment
                TeacherId = EditingCourse.TeacherId,
                Status = NewStatus
            };

            await CourseAppService.UpdateAsync(EditingCourse.Id, updateDto);
            await LoadMyCoursesAsync();
            await UpdateStatusModal.Hide();
            await Message.Success(L["SuccessfullyUpdated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // Navigation
    private void NavigateToGrades(CourseDto course)
    {
        NavigationManager.NavigateTo($"/teacher/grades?courseId={course.Id}");
    }

    private void NavigateToAttendances(CourseDto course)
    {
        NavigationManager.NavigateTo($"/teacher/attendances?courseId={course.Id}");
    }
}
