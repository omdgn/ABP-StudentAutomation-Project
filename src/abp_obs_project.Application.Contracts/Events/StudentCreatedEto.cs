using System;

namespace abp_obs_project.Events;

/// <summary>
/// Event Transfer Object for Student Creation
/// Published when a new student is created
/// Serializable for RabbitMQ/distributed event bus
/// </summary>
[Serializable]
public class StudentCreatedEto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }

    // Computed property for handlers
    public string FullName => $"{FirstName} {LastName}";
}
