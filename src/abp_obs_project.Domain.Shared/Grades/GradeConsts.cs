namespace abp_obs_project.Grades;

public static class GradeConsts
{
    public const double MinGradeValue = 0.0;
    public const double MaxGradeValue = 100.0;

    public const double PassingGrade = 50.0;

    public const int MaxCommentsLength = 1000;

    public static string GetDefaultSorting(bool withEntityName = false)
    {
        return withEntityName
            ? "Grade.GradeValue DESC"
            : "GradeValue DESC";
    }
}
