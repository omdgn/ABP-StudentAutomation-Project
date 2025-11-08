using System.Threading.Tasks;
using abp_obs_project.Localization;
using abp_obs_project.Permissions;
using Volo.Abp.UI.Navigation;

namespace abp_obs_project.Blazor.Menus.Teacher;

public class TeacherMenuContributor : IMenuContributor
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

        // Check if user is Admin (has both Students.ViewAll AND Teachers.ViewAll permissions)
        var hasStudentManagement = await context.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
        var hasTeacherManagement = await context.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);

        // If user has both permissions, they are admin - don't add teacher menu
        if (hasStudentManagement && hasTeacherManagement)
        {
            return;
        }

        // Check if user has any teacher-related permissions
        var hasCoursePermission = await context.IsGrantedAsync(abp_obs_projectPermissions.Courses.Default);
        var hasGradePermission = await context.IsGrantedAsync(abp_obs_projectPermissions.Grades.Default);

        // User must have at least one teacher permission
        if (!hasCoursePermission && !hasGradePermission)
        {
            return;
        }

        // Add Teacher menu items
        
        // Dashboard (Home)
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                abp_obs_projectMenus.TeacherDashboard,
                l["Menu:TeacherDashboard"],
                "/teacher/dashboard",
                icon: "fas fa-home",
                order: 0
            )
        );

        // My Courses
        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                abp_obs_projectMenus.TeacherMyCourses,
                l["Menu:MyCourses"],
                "/teacher/my-courses",
                icon: "fas fa-book",
                order: 1
            )
        );

        // My Students
        context.Menu.Items.Insert(
            2,
            new ApplicationMenuItem(
                abp_obs_projectMenus.TeacherMyStudents,
                l["Menu:MyStudents"],
                "/teacher/my-students",
                icon: "fas fa-user-graduate",
                order: 2
            )
        );

        // Settings
        context.Menu.Items.Insert(
            3,
            new ApplicationMenuItem(
                abp_obs_projectMenus.TeacherSettings,
                l["Menu:Settings"],
                "/teacher/settings",
                icon: "fas fa-cog",
                order: 3
            )
        );
    }
}
