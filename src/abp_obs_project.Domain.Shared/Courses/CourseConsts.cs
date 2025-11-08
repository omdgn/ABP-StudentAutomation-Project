namespace abp_obs_project.Courses;

public static class CourseConsts
{
    private const string DefaultSorting = "{0}Name asc";

    public static string GetDefaultSorting(bool withEntityName)
    {
        return string.Format(DefaultSorting, withEntityName ? "Course." : string.Empty);
    }

    public const int MaxNameLength = 256;
    public const int MinNameLength = 3;

    public const int MaxCodeLength = 20;
    public const int MinCodeLength = 3;

    public const int MaxDescriptionLength = 2000;

    public const int MinCredits = 1;
    public const int MaxCredits = 10;
}
