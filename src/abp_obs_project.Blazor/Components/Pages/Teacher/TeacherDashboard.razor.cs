using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Courses;
using abp_obs_project.Students;
using abp_obs_project.Enrollments;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherDashboard
{
    protected int TotalMyCourses { get; set; }
    protected int TotalMyStudents { get; set; }
    protected int MyActiveCourses { get; set; }
    protected bool IsTeacher { get; set; }
    protected List<StudentDto> RecentStudents { get; set; } = new();

    [Inject]
    protected IEnrollmentAppService EnrollmentAppService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Check if user is authenticated
        if (CurrentUser.Id.HasValue)
        {
            IsTeacher = true;
            await LoadStatisticsAsync();
        }
        else
        {
            IsTeacher = false;
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Load my courses - CourseAppService already filters by teacher email for non-admin users
            var myCoursesResult = await CourseAppService.GetListAsync(
                new GetCoursesInput
                {
                    MaxResultCount = 1000
                });

            var myCourses = myCoursesResult.Items.ToList();
            TotalMyCourses = myCourses.Count;

            // Get active courses count (InProgress status)
            MyActiveCourses = myCourses
                .Count(c => c.Status == EnumCourseStatus.InProgress);

            // Get all students enrolled in my courses using Enrollments
            var myStudentsSet = new HashSet<Guid>();
            var studentDetailsDict = new Dictionary<Guid, StudentDto>();

            foreach (var course in myCourses)
            {
                try
                {
                    // Get enrollments for this course
                    var enrollmentsResult = await EnrollmentAppService.GetListAsync(
                        new GetEnrollmentsInput
                        {
                            CourseId = course.Id,
                            Status = EnumEnrollmentStatus.Active,
                            MaxResultCount = 1000
                        });

                    foreach (var enrollment in enrollmentsResult.Items)
                    {
                        myStudentsSet.Add(enrollment.StudentId);

                        // Store student details from enrollment
                        if (!studentDetailsDict.ContainsKey(enrollment.StudentId))
                        {
                            studentDetailsDict[enrollment.StudentId] = new StudentDto
                            {
                                Id = enrollment.StudentId,
                                FirstName = enrollment.StudentName?.Split(' ')[0] ?? "",
                                LastName = enrollment.StudentName?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                                StudentNumber = enrollment.StudentNumber ?? ""
                            };
                        }
                    }
                }
                catch
                {
                    // If enrollment service fails, continue with other courses
                    continue;
                }
            }

            TotalMyStudents = myStudentsSet.Count;

            // Load recent students (last 5 unique students)
            if (myStudentsSet.Any())
            {
                var recentStudentIds = myStudentsSet.Take(5).ToList();
                RecentStudents = recentStudentIds
                    .Select(id => studentDetailsDict.TryGetValue(id, out var student) ? student : null)
                    .Where(s => s != null)
                    .ToList()!;
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
