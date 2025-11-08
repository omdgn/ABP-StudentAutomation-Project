using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Teachers;

/// <summary>
/// Lightweight DTO for teacher lookups (e.g., dropdown lists)
/// </summary>
public class TeacherLookupDto : EntityDto<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
}
