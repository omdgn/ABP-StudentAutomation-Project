namespace abp_obs_project.Permissions;

public static class abp_obs_projectPermissions
{
    public const string GroupName = "abp_obs_project";

    public static class Students
    {
        public const string Default = GroupName + ".Students";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class Teachers
    {
        public const string Default = GroupName + ".Teachers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class Courses
    {
        public const string Default = GroupName + ".Courses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
        public const string ManageStudents = Default + ".ManageStudents";
        public const string UpdateStatus = Default + ".UpdateStatus";
    }

    public static class Grades
    {
        public const string Default = GroupName + ".Grades";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class Attendances
    {
        public const string Default = GroupName + ".Attendances";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class Enrollments
    {
        public const string Default = GroupName + ".Enrollments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAll = Default + ".ViewAll";
    }
}
