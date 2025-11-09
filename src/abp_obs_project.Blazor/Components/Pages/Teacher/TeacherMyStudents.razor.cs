using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Students;
using abp_obs_project.Permissions;
using abp_obs_project.Courses;
using abp_obs_project.Enrollments;
using abp_obs_project.Grades;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherMyStudents
{
    [Inject] public IStudentAppService StudentAppService { get; set; } = default!;
    [Inject] public ICourseAppService CourseAppService { get; set; } = default!;
    [Inject] public IEnrollmentAppService EnrollmentAppService { get; set; } = default!;
    [Inject] public IGradeAppService GradeAppService { get; set; } = default!;
    [Inject] public new IAuthorizationService AuthorizationService { get; set; } = default!;

    protected List<StudentDto> MyStudents { get; set; } = new();
    protected List<CourseDto> MyCourses { get; set; } = new();
    protected Dictionary<Guid, List<EnrollmentDto>> CourseEnrollments { get; set; } = new();
    protected bool IsTeacher { get; set; }
    private bool HasEnrollmentPermission { get; set; }
    private bool CanCreateEnrollment { get; set; }
    private bool CanDeleteEnrollment { get; set; }
    private bool HasGradesPermission { get; set; }

    // Modal
    private Modal StudentDetailsModal { get; set; } = null!;
    private StudentDto? SelectedStudent { get; set; }

    // Add Student to Course modal
    private Modal AddStudentModal { get; set; } = null!;
    private List<StudentLookup> AvailableStudentsToAdd { get; set; } = new();
    private Guid? SelectedStudentToAdd { get; set; }
    private Guid SelectedCourseIdForAdd { get; set; }
    private bool IsSelectedStudentAlreadyEnrolled =>
        SelectedStudentToAdd.HasValue &&
        AvailableStudentsToAdd.Any(x => x.Id == SelectedStudentToAdd.Value && x.IsEnrolled);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is a teacher
        IsTeacher = await IsTeacherAsync();

        if (IsTeacher)
        {
            HasEnrollmentPermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Enrollments.Default);
            CanCreateEnrollment = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Enrollments.Create);
            CanDeleteEnrollment = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Enrollments.Delete);
            HasGradesPermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Default);
            await LoadMyStudentsAsync();
        }
    }

    private async Task<bool> IsTeacherAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return false;
        }

        // Check if user is Admin (has both Students.ViewAll AND Teachers.ViewAll permissions)
        var hasStudentManagement = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
        var hasTeacherManagement = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);

        // If user has both permissions, they are admin - not a teacher
        if (hasStudentManagement && hasTeacherManagement)
        {
            return false;
        }

        // Check if user has any teacher-related permissions
        var hasCoursePermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default);
        var hasGradePermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Grades.Default);
        var hasAttendancePermission = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Default);

        // Teacher must have at least one of these permissions
        return hasCoursePermission || hasGradePermission || hasAttendancePermission;
    }

    private async Task LoadMyStudentsAsync()
    {
        try
        {
            // Get teacher's courses: The service automatically filters based on Courses.ViewAll permission
            var coursesResult = await CourseAppService.GetListAsync(new GetCoursesInput { MaxResultCount = 1000 });
            MyCourses = coursesResult.Items.ToList();
            var myCourseIds = MyCourses.Select(c => c.Id).ToList();

            Logger.LogInformation($"Teacher has {MyCourses.Count} courses");
            foreach (var course in MyCourses)
            {
                Logger.LogInformation($"Course: {course.Name} ({course.Code}) - ID: {course.Id}");
            }

            if (!myCourseIds.Any())
            {
                // Teacher has no courses
                Logger.LogWarning("Teacher has no courses assigned");
                return;
            }

            var allEnrollments = new List<EnrollmentDto>();
            CourseEnrollments.Clear();

            if (HasEnrollmentPermission)
            {
                // Preferred path: use enrollments API
                Logger.LogInformation("Using Enrollment permission path");
                foreach (var courseId in myCourseIds)
                {
                    var enrollmentsResult = await EnrollmentAppService.GetListAsync(new GetEnrollmentsInput
                    {
                        MaxResultCount = 1000,
                        CourseId = courseId,
                        Status = EnumEnrollmentStatus.Active
                    });
                    var list = enrollmentsResult.Items.ToList();
                    Logger.LogInformation($"Course {courseId}: Found {list.Count} enrollments");
                    CourseEnrollments[courseId] = list;
                    allEnrollments.AddRange(list);
                }
            }
            else if (HasGradesPermission)
            {
                // Fallback: derive enrollments from grades
                foreach (var courseId in myCourseIds)
                {
                    var grades = await GradeAppService.GetListAsync(new GetGradesInput
                    {
                        MaxResultCount = 1000,
                        CourseId = courseId
                    });
                    var byStudent = grades.Items
                        .GroupBy(g => new { g.StudentId, g.StudentName })
                        .Select(g => new EnrollmentDto
                        {
                            Id = Guid.Empty,
                            StudentId = g.Key.StudentId,
                            CourseId = courseId,
                            EnrolledAt = DateTime.MinValue,
                            Status = EnumEnrollmentStatus.Active,
                            StudentName = g.Key.StudentName,
                            CourseName = MyCourses.First(x => x.Id == courseId).Name,
                            CourseCode = MyCourses.First(x => x.Id == courseId).Code
                        })
                        .ToList();
                    CourseEnrollments[courseId] = byStudent;
                    allEnrollments.AddRange(byStudent);
                }
            }

            // Get unique student IDs from enrollments
            var studentIds = allEnrollments.Select(e => e.StudentId).Distinct().ToList();

            if (!studentIds.Any())
            {
                // No students enrolled
                return;
            }

            // Get student details (only those needed) without requiring Students.ViewAll
            var students = new List<StudentDto>();
            foreach (var id in studentIds)
            {
                students.Add(await StudentAppService.GetAsync(id));
            }
            MyStudents = students;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task OpenAddStudentModal(Guid courseId)
    {
        SelectedCourseIdForAdd = courseId;

        var allStudents = await StudentAppService.GetListAsync(new GetStudentsInput { MaxResultCount = 1000 });
        var enrolledIds = CourseEnrollments.TryGetValue(courseId, out var enrollments)
            ? enrollments.Select(e => e.StudentId).ToHashSet()
            : new HashSet<Guid>();

        // Show all students, mark enrolled ones
        AvailableStudentsToAdd = allStudents.Items
            .Select(s => new StudentLookup(
                s.Id,
                $"{s.FirstName} {s.LastName} ({s.StudentNumber})",
                enrolledIds.Contains(s.Id)))
            .OrderBy(s => s.DisplayName)
            .ToList();

        SelectedStudentToAdd = AvailableStudentsToAdd.Count > 0 ? AvailableStudentsToAdd[0].Id : (Guid?)null;
        await AddStudentModal.Show();
    }

    private async Task CloseAddStudentModal()
    {
        await AddStudentModal.Hide();
        SelectedStudentToAdd = null;
        AvailableStudentsToAdd.Clear();
    }

    private async Task AddStudentToCourseAsync()
    {
        if (SelectedStudentToAdd == null || IsSelectedStudentAlreadyEnrolled)
        {
            return;
        }

        try
        {
            if (HasEnrollmentPermission && CanCreateEnrollment)
            {
                await EnrollmentAppService.CreateAsync(new CreateEnrollmentDto
                {
                    StudentId = SelectedStudentToAdd.Value,
                    CourseId = SelectedCourseIdForAdd
                });
            }
            else if (HasGradesPermission)
            {
                // Fallback: create a zero-grade to mark enrollment
                await GradeAppService.CreateAsync(new CreateUpdateGradeDto
                {
                    StudentId = SelectedStudentToAdd.Value,
                    CourseId = SelectedCourseIdForAdd,
                    GradeValue = 0.0,
                    Comments = "Enrolled"
                });
            }

            await LoadMyStudentsAsync();
            await CloseAddStudentModal();
            await Message.Success(L["StudentAddedToCourse"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task RemoveStudentFromCourseAsync(Guid enrollmentId)
    {
        try
        {
            var confirmed = await Message.Confirm(L["RemoveStudentConfirmation"]);
            if (!confirmed)
            {
                return;
            }

            if (HasEnrollmentPermission && CanDeleteEnrollment)
            {
                await EnrollmentAppService.WithdrawAsync(enrollmentId);
            }
            else if (HasGradesPermission)
            {
                // Fallback: remove any grades for this student and course
                var course = MyCourses.FirstOrDefault(c => CourseEnrollments.TryGetValue(c.Id, out var list) && list.Any(e => e.Id == enrollmentId));
                if (course != null)
                {
                    var grades = await GradeAppService.GetListAsync(new GetGradesInput
                    {
                        MaxResultCount = 1000,
                        CourseId = course.Id,
                        StudentId = CourseEnrollments[course.Id].First(e => e.Id == enrollmentId).StudentId
                    });
                    foreach (var g in grades.Items)
                    {
                        await GradeAppService.DeleteAsync(g.Id);
                    }
                }
            }

            await LoadMyStudentsAsync();
            await Message.Success(L["StudentRemovedFromCourse"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private readonly record struct StudentLookup(Guid Id, string DisplayName, bool IsEnrolled);

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
