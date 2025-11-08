using System;

namespace abp_obs_project.Events;

/// <summary>
/// Event Transfer Object for Student Deletion
/// Published when a student is deleted
/// </summary>
[Serializable]
public class StudentDeletedEto
{
    public Guid Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime DeletionTime { get; set; }
}
