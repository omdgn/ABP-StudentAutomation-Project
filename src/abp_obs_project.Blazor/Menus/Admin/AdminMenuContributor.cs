using System.Threading.Tasks;
using abp_obs_project.Localization;
using abp_obs_project.MultiTenancy;
using abp_obs_project.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;

namespace abp_obs_project.Blazor.Menus.Admin;

public class AdminMenuContributor : IMenuContributor
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
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<abp_obs_projectResource>();

        // Check if user has admin permissions
        // For "admin" users, ABP's data seeder grants all permissions automatically
        var hasStudentManagement = await context.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
        var hasTeacherManagement = await context.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);

        // User is admin if they have BOTH student AND teacher management permissions
        // The built-in admin user gets all permissions from ABP's IdentityDataSeeder
        var isAdmin = hasStudentManagement && hasTeacherManagement;

        if (!isAdmin)
        {
            // User is not admin, don't add admin menu items
            // Hide administration menu as well
            context.Menu.TryRemoveMenuItem(administration.Name);
            return;
        }

        // Dashboard (Home) - combined as per user request
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Dashboard,
                l["Menu:Dashboard"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        // Students - now a top-level menu item
        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Students,
                l["Menu:Students"],
                "/admin/students",
                icon: "fas fa-user-graduate",
                order: 1,
                requiredPermissionName: abp_obs_projectPermissions.Students.ViewAll
            )
        );

        // Teachers - now a top-level menu item
        context.Menu.Items.Insert(
            2,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Teachers,
                l["Menu:Teachers"],
                "/admin/teachers",
                icon: "fas fa-chalkboard-teacher",
                order: 2,
                requiredPermissionName: abp_obs_projectPermissions.Teachers.ViewAll
            )
        );

        // Courses - now a top-level menu item
        context.Menu.Items.Insert(
            3,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Courses,
                l["Menu:Courses"],
                "/admin/courses",
                icon: "fas fa-book",
                order: 3,
                requiredPermissionName: abp_obs_projectPermissions.Courses.ViewAll
            )
        );

        // Grades - now a top-level menu item
        context.Menu.Items.Insert(
            4,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Grades,
                l["Menu:Grades"],
                "/admin/grades",
                icon: "fas fa-star",
                order: 4,
                requiredPermissionName: abp_obs_projectPermissions.Grades.ViewAll
            )
        );

        // Attendances - now a top-level menu item
        context.Menu.Items.Insert(
            5,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Attendances,
                l["Menu:Attendances"],
                "/admin/attendances",
                icon: "fas fa-calendar-check",
                order: 5,
                requiredPermissionName: abp_obs_projectPermissions.Attendances.ViewAll
            )
        );

        // Administration group (Identity, Settings, etc.) - order 6
        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);
        administration.Order = 6;
    }
}
