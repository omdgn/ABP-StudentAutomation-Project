namespace abp_obs_project.Attendances;

public static class AttendanceConsts
{
    public const int MaxRemarksLength = 500;

    public static string GetDefaultSorting(bool withEntityName = false)
    {
        return withEntityName
            ? "Attendance.AttendanceDate DESC"
            : "AttendanceDate DESC";
    }
}
