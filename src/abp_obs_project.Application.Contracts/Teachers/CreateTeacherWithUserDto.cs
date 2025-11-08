using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Teachers;

/// <summary>
/// DTO for creating a teacher along with their identity user account
/// </summary>
public class CreateTeacherWithUserDto
{
    // Teacher Information
    [Required]
    [StringLength(TeacherConsts.MaxFirstNameLength, MinimumLength = TeacherConsts.MinFirstNameLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(TeacherConsts.MaxLastNameLength, MinimumLength = TeacherConsts.MinLastNameLength)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(TeacherConsts.MaxEmailLength, MinimumLength = TeacherConsts.MinEmailLength)]
    public string Email { get; set; } = string.Empty;

    [StringLength(TeacherConsts.MaxDepartmentLength)]
    public string? Department { get; set; }

    [Phone]
    [StringLength(TeacherConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    // Identity User Information
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
