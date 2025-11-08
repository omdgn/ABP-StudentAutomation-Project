using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Attendances;

/// <summary>
/// Attendance Application Service
/// Inherits from ICrudAppService for standard CRUD operations (ABP best practice)
/// </summary>
public interface IAttendanceAppService
    : ICrudAppService<AttendanceDto, Guid, GetAttendancesInput, CreateUpdateAttendanceDto, CreateUpdateAttendanceDto>
{
    /// <summary>
    /// Gets absence count for a specific student in a specific course
    /// </summary>
    Task<int> GetAbsenceCountAsync(Guid studentId, Guid courseId);

    /// <summary>
    /// Gets attendance rate (percentage) for a specific student in a specific course
    /// </summary>
    Task<double> GetAttendanceRateAsync(Guid studentId, Guid courseId);
}
