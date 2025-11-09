using System;

namespace abp_obs_project.Students;

/// <summary>
/// DTO for a student to update own profile.
/// Email and StudentNumber are intentionally excluded.
/// </summary>
public class UpdateMyProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public EnumGender Gender { get; set; } = EnumGender.Unknown;
    public string? Phone { get; set; }
}

