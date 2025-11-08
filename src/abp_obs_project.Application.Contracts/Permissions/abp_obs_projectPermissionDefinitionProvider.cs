using abp_obs_project.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace abp_obs_project.Permissions;

public class abp_obs_projectPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var obsGroup = context.AddGroup(abp_obs_projectPermissions.GroupName, L("Permission:ObsManagement"));

        // Student Management
        var studentsPermission = obsGroup.AddPermission(
            abp_obs_projectPermissions.Students.Default,
            L("Permission:Students"));
        studentsPermission.AddChild(
            abp_obs_projectPermissions.Students.Create,
            L("Permission:Students.Create"));
        studentsPermission.AddChild(
            abp_obs_projectPermissions.Students.Edit,
            L("Permission:Students.Edit"));
        studentsPermission.AddChild(
            abp_obs_projectPermissions.Students.Delete,
            L("Permission:Students.Delete"));
        studentsPermission.AddChild(
            abp_obs_projectPermissions.Students.ViewAll,
            L("Permission:Students.ViewAll"));

        // Teacher Management
        var teachersPermission = obsGroup.AddPermission(
            abp_obs_projectPermissions.Teachers.Default,
            L("Permission:Teachers"));
        teachersPermission.AddChild(
            abp_obs_projectPermissions.Teachers.Create,
            L("Permission:Teachers.Create"));
        teachersPermission.AddChild(
            abp_obs_projectPermissions.Teachers.Edit,
            L("Permission:Teachers.Edit"));
        teachersPermission.AddChild(
            abp_obs_projectPermissions.Teachers.Delete,
            L("Permission:Teachers.Delete"));
        teachersPermission.AddChild(
            abp_obs_projectPermissions.Teachers.ViewAll,
            L("Permission:Teachers.ViewAll"));

        // Course Management
        var coursesPermission = obsGroup.AddPermission(
            abp_obs_projectPermissions.Courses.Default,
            L("Permission:Courses"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.Create,
            L("Permission:Courses.Create"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.Edit,
            L("Permission:Courses.Edit"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.Delete,
            L("Permission:Courses.Delete"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.ViewAll,
            L("Permission:Courses.ViewAll"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.ManageStudents,
            L("Permission:Courses.ManageStudents"));
        coursesPermission.AddChild(
            abp_obs_projectPermissions.Courses.UpdateStatus,
            L("Permission:Courses.UpdateStatus"));

        // Grade Management
        var gradesPermission = obsGroup.AddPermission(
            abp_obs_projectPermissions.Grades.Default,
            L("Permission:Grades"));
        gradesPermission.AddChild(
            abp_obs_projectPermissions.Grades.Create,
            L("Permission:Grades.Create"));
        gradesPermission.AddChild(
            abp_obs_projectPermissions.Grades.Edit,
            L("Permission:Grades.Edit"));
        gradesPermission.AddChild(
            abp_obs_projectPermissions.Grades.Delete,
            L("Permission:Grades.Delete"));
        gradesPermission.AddChild(
            abp_obs_projectPermissions.Grades.ViewAll,
            L("Permission:Grades.ViewAll"));

        // Attendance Management
        var attendancesPermission = obsGroup.AddPermission(
            abp_obs_projectPermissions.Attendances.Default,
            L("Permission:Attendances"));
        attendancesPermission.AddChild(
            abp_obs_projectPermissions.Attendances.Create,
            L("Permission:Attendances.Create"));
        attendancesPermission.AddChild(
            abp_obs_projectPermissions.Attendances.Edit,
            L("Permission:Attendances.Edit"));
        attendancesPermission.AddChild(
            abp_obs_projectPermissions.Attendances.Delete,
            L("Permission:Attendances.Delete"));
        attendancesPermission.AddChild(
            abp_obs_projectPermissions.Attendances.ViewAll,
            L("Permission:Attendances.ViewAll"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<abp_obs_projectResource>(name);
    }
}
