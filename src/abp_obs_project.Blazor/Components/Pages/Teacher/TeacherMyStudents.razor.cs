using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Students;
using abp_obs_project.Permissions;
using abp_obs_project.Teachers;
using abp_obs_project.Courses;
using abp_obs_project.Grades;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherMyStudents
{
    [Inject] public IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] public ITeacherAppService TeacherAppService { get; set; } = default!;
    [Inject] public ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] public IGradeAppService GradeAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected List<StudentDto> MyStudents { get; set; } = new();
    protected bool IsTeacher { get; set; }
    
    // Modal
    private Modal StudentDetailsModal { get; set; } = null!;
    private StudentDto? SelectedStudent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is a teacher
        IsTeacher = await IsTeacherAsync();

        if (IsTeacher)
        {
            await LoadMyStudentsAsync();
        }
    }

    private async Task<bool> IsTeacherAsync()
    {
        // Teacher should have Student.Default permission but NOT ViewAll
        var hasStudentPermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.Default);
        var hasViewAllPermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);

        // If user is authenticated and has Student permission but NOT ViewAll, they are teacher
        return CurrentUser.IsAuthenticated && hasStudentPermission && !hasViewAllPermission;
    }

    private async Task LoadMyStudentsAsync()
    {
        try
        {
            // Find current teacher by email
            var currentEmail = CurrentUser.Email;
            if (string.IsNullOrEmpty(currentEmail))
            {
                return;
            }

            // Get current teacher
            var teachersResult = await TeacherAppService.GetListAsync(new GetTeachersInput
            {
                MaxResultCount = 1000,
                Email = currentEmail
            });

            var currentTeacher = teachersResult.Items.FirstOrDefault();
            if (currentTeacher == null)
            {
                return;
            }

            // Get teacher's courses
            var coursesResult = await CourseAppService.GetListAsync(new GetCoursesInput
            {
                MaxResultCount = 1000,
                TeacherId = currentTeacher.Id
            });

            var myCourseIds = coursesResult.Items.Select(c => c.Id).ToList();

            if (!myCourseIds.Any())
            {
                // Teacher has no courses
                return;
            }

            // Get all grades for teacher's courses
            var allGrades = new List<GradeDto>();
            foreach (var courseId in myCourseIds)
            {
                var gradesResult = await GradeAppService.GetListAsync(new GetGradesInput
                {
                    MaxResultCount = 1000,
                    CourseId = courseId
                });
                allGrades.AddRange(gradesResult.Items);
            }

            // Get unique student IDs from grades
            var studentIds = allGrades.Select(g => g.StudentId).Distinct().ToList();

            if (!studentIds.Any())
            {
                // No students enrolled
                return;
            }

            // Get student details
            var studentsResult = await StudentAppService.GetListAsync(new GetStudentsInput
            {
                MaxResultCount = 1000
            });

            // Filter students by IDs
            MyStudents = studentsResult.Items
                .Where(s => studentIds.Contains(s.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
    
    private async Task OpenStudentDetailsModal(StudentDto student)
    {
        SelectedStudent = student;
        await StudentDetailsModal.Show();
    }
    
    private async Task CloseStudentDetailsModal()
    {
        await StudentDetailsModal.Hide();
        SelectedStudent = null;
    }
}
