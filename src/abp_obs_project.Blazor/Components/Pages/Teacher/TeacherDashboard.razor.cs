using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using abp_obs_project.Courses;
using abp_obs_project.Students;
using abp_obs_project.Grades;

namespace abp_obs_project.Blazor.Components.Pages.Teacher;

public partial class TeacherDashboard
{
    protected int TotalMyCourses { get; set; }
    protected int TotalMyStudents { get; set; }
    protected int MyActiveCourses { get; set; }
    protected bool IsTeacher { get; set; }
    protected List<StudentDto> RecentStudents { get; set; } = new();

    [Inject]
    protected IGradeAppService GradeAppService { get; set; } = default!;

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
            var currentUserId = CurrentUser.Id!.Value;

            // Load my courses (courses where I am the teacher)
            var myCoursesResult = await CourseAppService.GetListAsync(
                new GetCoursesInput
                {
                    MaxResultCount = 1000  // Get all courses to filter by teacher
                });

            // Filter courses by current teacher's ID
            var myCourses = myCoursesResult.Items
                .Where(c => c.TeacherId == currentUserId)
                .ToList();

            TotalMyCourses = myCourses.Count;

            // Get active courses count (InProgress status)
            MyActiveCourses = myCourses
                .Count(c => c.Status == EnumCourseStatus.InProgress);

            // Get all students enrolled in my courses
            // We'll use Grade records to find unique students
            var myStudentsSet = new HashSet<Guid>();

            foreach (var course in myCourses)
            {
                try
                {
                    // Get grades for this course to find enrolled students
                    var gradesResult = await GradeAppService.GetListAsync(
                        new GetGradesInput
                        {
                            CourseId = course.Id,
                            MaxResultCount = 1000
                        });

                    foreach (var grade in gradesResult.Items)
                    {
                        myStudentsSet.Add(grade.StudentId);
                    }
                }
                catch
                {
                    // If grades service fails, continue with other courses
                    continue;
                }
            }

            TotalMyStudents = myStudentsSet.Count;

            // Load recent students (last 5 unique students)
            if (myStudentsSet.Any())
            {
                var recentStudentIds = myStudentsSet.Take(5).ToList();
                var studentsList = new List<StudentDto>();

                foreach (var studentId in recentStudentIds)
                {
                    try
                    {
                        var studentsResult = await StudentAppService.GetListAsync(
                            new GetStudentsInput
                            {
                                MaxResultCount = 1000
                            });

                        var student = studentsResult.Items.FirstOrDefault(s => s.Id == studentId);
                        if (student != null)
                        {
                            studentsList.Add(student);
                        }
                    }
                    catch
                    {
                        // If student service fails, continue
                        continue;
                    }
                }

                RecentStudents = studentsList;
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
