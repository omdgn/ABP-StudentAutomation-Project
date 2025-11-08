using abp_obs_project.Attendances;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// UI Service for Attendance operations
/// Provides abstraction layer between Blazor components and AppService
/// </summary>
public interface IAttendanceUIService
    : IUIServiceBase<AttendanceDto, GetAttendancesInput, CreateUpdateAttendanceDto>
{
    // Attendance-specific methods can be added here if needed
    // Example: Task<int> GetStudentAbsenceCountAsync(Guid studentId, Guid courseId);
}
