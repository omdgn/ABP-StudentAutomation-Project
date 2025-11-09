namespace abp_obs_project.Blazor.Menus;

public class abp_obs_projectMenus
{
    private const string Prefix = "abp_obs_project";
    public const string Home = Prefix + ".Home";

    // Admin Menu
    public const string Dashboard = Prefix + ".Dashboard";
    public const string Management = Prefix + ".Management";
    public const string Students = Prefix + ".Students";
    public const string Teachers = Prefix + ".Teachers";
    public const string Courses = Prefix + ".Courses";
    public const string Grades = Prefix + ".Grades";
    public const string Attendances = Prefix + ".Attendances";

    // Teacher Menu
    public const string TeacherDashboard = Prefix + ".TeacherDashboard";
    public const string TeacherMyCourses = Prefix + ".TeacherMyCourses";
    public const string TeacherMyStudents = Prefix + ".TeacherMyStudents";
    public const string TeacherGrades = Prefix + ".TeacherGrades";
    public const string TeacherAttendances = Prefix + ".TeacherAttendances";
    public const string TeacherSettings = Prefix + ".TeacherSettings";

    // Student Menu
    public const string StudentDashboard = Prefix + ".StudentDashboard";
    public const string StudentMyCourses = Prefix + ".StudentMyCourses";
    public const string StudentGrades = Prefix + ".StudentGrades";
    public const string StudentAttendances = Prefix + ".StudentAttendances";
    public const string StudentProfile = Prefix + ".StudentProfile";
}
