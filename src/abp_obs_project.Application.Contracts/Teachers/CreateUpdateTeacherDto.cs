using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Teachers;

/// <summary>
/// DTO for creating or updating a teacher
/// </summary>
public class CreateUpdateTeacherDto
{
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
}
