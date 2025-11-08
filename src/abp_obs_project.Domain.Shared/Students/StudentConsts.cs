namespace abp_obs_project.Students;

public static class StudentConsts
{
    private const string DefaultSorting = "{0}FirstName asc";

    public static string GetDefaultSorting(bool withEntityName)
    {
        return string.Format(DefaultSorting, withEntityName ? "Student." : string.Empty);
    }

    public const int MaxFirstNameLength = 128;
    public const int MinFirstNameLength = 2;

    public const int MaxLastNameLength = 128;
    public const int MinLastNameLength = 2;

    public const int MaxEmailLength = 256;
    public const int MinEmailLength = 5;

    public const int MaxStudentNumberLength = 20;
    public const int MinStudentNumberLength = 5;

    public const int MaxPhoneLength = 20;
}
