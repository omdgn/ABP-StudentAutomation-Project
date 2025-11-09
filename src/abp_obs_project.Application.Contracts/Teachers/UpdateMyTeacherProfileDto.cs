namespace abp_obs_project.Teachers;

/// <summary>
/// DTO for a teacher to update own profile.
/// Email is not editable here.
/// </summary>
public class UpdateMyTeacherProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
}

