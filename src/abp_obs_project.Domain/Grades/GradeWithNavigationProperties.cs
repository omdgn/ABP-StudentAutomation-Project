using abp_obs_project.Courses;
using abp_obs_project.Students;

namespace abp_obs_project.Grades;

/// <summary>
/// Grade with its related navigation properties
/// Used for queries that need to include related entities
/// </summary>
public class GradeWithNavigationProperties
{
    public Grade Grade { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
