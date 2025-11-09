using abp_obs_project.Courses;
using abp_obs_project.Students;

namespace abp_obs_project.Enrollments;

/// <summary>
/// Enrollment with its related navigation properties
/// Used for queries that need to include related entities
/// </summary>
public class EnrollmentWithNavigationProperties
{
    public Enrollment Enrollment { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
