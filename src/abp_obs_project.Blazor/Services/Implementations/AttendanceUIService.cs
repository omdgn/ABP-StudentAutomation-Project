using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Attendances;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// UI Service implementation for Attendance operations
/// Inherits all CRUD operations from UIServiceBase
/// </summary>
public class AttendanceUIService
    : UIServiceBase<IAttendanceAppService, AttendanceDto, GetAttendancesInput, CreateUpdateAttendanceDto>,
      IAttendanceUIService
{
    public AttendanceUIService(
        IAttendanceAppService attendanceAppService,
        ILogger<AttendanceUIService> logger)
        : base(attendanceAppService, logger, "Attendance")
    {
    }

    // Attendance-specific methods can be added here
}
