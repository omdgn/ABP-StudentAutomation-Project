using System;
using System.ComponentModel.DataAnnotations;

namespace abp_obs_project.Students;

/// <summary>
/// DTO for creating a student along with their identity user account
/// </summary>
public class CreateStudentWithUserDto
{
    // Student Information
    [Required]
    [StringLength(StudentConsts.MaxFirstNameLength, MinimumLength = StudentConsts.MinFirstNameLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(StudentConsts.MaxLastNameLength, MinimumLength = StudentConsts.MinLastNameLength)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(StudentConsts.MaxEmailLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(StudentConsts.MaxStudentNumberLength)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    public EnumGender Gender { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Phone]
    [StringLength(StudentConsts.MaxPhoneLength)]
    public string? Phone { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; }

    public Guid? TeacherId { get; set; }

    // Identity User Information
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
