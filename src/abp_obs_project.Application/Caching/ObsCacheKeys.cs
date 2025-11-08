namespace abp_obs_project.Caching;

/// <summary>
/// Centralized cache key constants for OBS entities
/// </summary>
public static class ObsCacheKeys
{
    private const string Prefix = "obs";

    public static class Students
    {
        public const string List = $"{Prefix}:students:list";
        public static string Item(string id) => $"{Prefix}:students:item:{id}";
    }

    public static class Teachers
    {
        public const string List = $"{Prefix}:teachers:list";
        public static string Item(string id) => $"{Prefix}:teachers:item:{id}";
    }

    public static class Courses
    {
        public const string List = $"{Prefix}:courses:list";
        public static string Item(string id) => $"{Prefix}:courses:item:{id}";
    }
}
