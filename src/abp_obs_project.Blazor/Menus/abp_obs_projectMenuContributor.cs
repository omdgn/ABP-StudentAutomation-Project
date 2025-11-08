using System.Threading.Tasks;
using abp_obs_project.Localization;
using abp_obs_project.MultiTenancy;
using abp_obs_project.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;

namespace abp_obs_project.Blazor.Menus;

public class abp_obs_projectMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<abp_obs_projectResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                abp_obs_projectMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        // Student Management Menu
        context.Menu.AddItem(
            new ApplicationMenuItem(
                abp_obs_projectMenus.Students,
                l["Menu:Students"],
                "/students",
                icon: "fas fa-user-graduate",
                order: 1,
                requiredPermissionName: abp_obs_projectPermissions.Students.Default
            )
        );

        // Teacher Management Menu
        context.Menu.AddItem(
            new ApplicationMenuItem(
                abp_obs_projectMenus.Teachers,
                l["Menu:Teachers"],
                "/teachers",
                icon: "fas fa-chalkboard-teacher",
                order: 2,
                requiredPermissionName: abp_obs_projectPermissions.Teachers.Default
            )
        );

        // Course Management Menu
        context.Menu.AddItem(
            new ApplicationMenuItem(
                abp_obs_projectMenus.Courses,
                l["Menu:Courses"],
                "/courses",
                icon: "fas fa-book",
                order: 3,
                requiredPermissionName: abp_obs_projectPermissions.Courses.Default
            )
        );

        // Grade Management Menu
        context.Menu.AddItem(
            new ApplicationMenuItem(
                abp_obs_projectMenus.Grades,
                l["Menu:Grades"],
                "/grades",
                icon: "fas fa-clipboard-list",
                order: 4,
                requiredPermissionName: abp_obs_projectPermissions.Grades.Default
            )
        );

        // Attendance Management Menu
        context.Menu.AddItem(
            new ApplicationMenuItem(
                abp_obs_projectMenus.Attendances,
                l["Menu:Attendances"],
                "/attendances",
                icon: "fas fa-calendar-check",
                order: 5,
                requiredPermissionName: abp_obs_projectPermissions.Attendances.Default
            )
        );

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

        return Task.CompletedTask;
    }
}
