using abp_obs_project.Teachers;

namespace abp_obs_project.Courses;

/// <summary>
/// Course with its related navigation properties
/// Used for queries that need to include related entities
/// </summary>
public class CourseWithNavigationProperties
{
    public Course Course { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
}
