using System;

namespace abp_obs_project.Events;

/// <summary>
/// Event Transfer Object for Attendance Recording
/// Published when attendance is taken
/// Triggers: Absence alert if threshold exceeded, attendance reports update, etc.
/// </summary>
[Serializable]
public class AttendanceRecordedEto
{
    public Guid AttendanceId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; }

    // Additional info for handlers
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int TotalAbsenceCount { get; set; }  // For threshold checks
}
