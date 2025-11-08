using System;

namespace abp_obs_project.Events;

/// <summary>
/// Event Transfer Object for Student Update
/// Published when a student is updated
/// </summary>
[Serializable]
public class StudentUpdatedEto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime LastModificationTime { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
