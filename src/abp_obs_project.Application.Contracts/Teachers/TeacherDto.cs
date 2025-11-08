using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Teachers;

/// <summary>
/// Teacher data transfer object
/// </summary>
public class TeacherDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
}
