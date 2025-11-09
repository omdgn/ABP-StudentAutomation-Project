using System.Threading.Tasks;
using abp_obs_project.Localization;
using abp_obs_project.Permissions;
using Volo.Abp.UI.Navigation;

namespace abp_obs_project.Blazor.Menus.Student;

public class StudentMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<abp_obs_projectResource>();

        // Check if user is Admin or Teacher (if yes, don't show student menu)
        var hasStudentsViewAll = await context.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
        var hasTeachersViewAll = await context.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);
        var hasGradesCreate = await context.IsGrantedAsync(abp_obs_projectPermissions.Grades.Create);
        var hasAttendancesCreate = await context.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Create);
        var hasCoursesCreate = await context.IsGrantedAsync(abp_obs_projectPermissions.Courses.Create);

        // If user has admin or teacher permissions, they are NOT a student
        if (hasStudentsViewAll || hasTeachersViewAll || hasGradesCreate || hasAttendancesCreate || hasCoursesCreate)
        {
            return;
        }

        // Check if user has basic student permissions
        var hasStudents = await context.IsGrantedAsync(abp_obs_projectPermissions.Students.Default);
        var hasCourses = await context.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default);
        var hasGrades = await context.IsGrantedAsync(abp_obs_projectPermissions.Grades.Default);
        var hasAttendances = await context.IsGrantedAsync(abp_obs_projectPermissions.Attendances.Default);

        // Student must have at least basic view permissions
        if (!hasStudents && !hasCourses && !hasGrades && !hasAttendances)
        {
            return;
        }

        // User is a student, show student menu
        // Add Student menu items

        // Dashboard (Home)
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                abp_obs_projectMenus.StudentDashboard,
                l["Menu:StudentDashboard"],
                "/student/dashboard",
                icon: "fas fa-home",
                order: 0
            )
        );

        // My Courses
        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                abp_obs_projectMenus.StudentMyCourses,
                l["Menu:StudentMyCourses"],
                "/student/my-courses",
                icon: "fas fa-book",
                order: 1
            )
        );

        // My Grades
        context.Menu.Items.Insert(
            2,
            new ApplicationMenuItem(
                abp_obs_projectMenus.StudentGrades,
                l["Menu:StudentGrades"],
                "/student/grades",
                icon: "fas fa-star",
                order: 2
            )
        );

        // My Attendances
        context.Menu.Items.Insert(
            3,
            new ApplicationMenuItem(
                abp_obs_projectMenus.StudentAttendances,
                l["Menu:StudentAttendances"],
                "/student/attendances",
                icon: "fas fa-calendar-check",
                order: 3
            )
        );

        // Profile
        context.Menu.Items.Insert(
            4,
            new ApplicationMenuItem(
                abp_obs_projectMenus.StudentProfile,
                l["Menu:StudentProfile"],
                "/student/profile",
                icon: "fas fa-user",
                order: 4
            )
        );
    }
}
