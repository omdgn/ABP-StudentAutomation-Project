using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Students;

public class StudentDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public EnumGender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }

    // Enrollment & Teacher
    public DateTime EnrollmentDate { get; set; }
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }

    // Address
    public string? Address { get; set; }

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
}
