namespace abp_obs_project.Enrollments;

public static class EnrollmentConsts
{
    public static string GetDefaultSorting(bool withEntityName = false)
    {
        return withEntityName
            ? "Enrollment.EnrolledAt DESC"
            : "EnrolledAt DESC";
    }
}
