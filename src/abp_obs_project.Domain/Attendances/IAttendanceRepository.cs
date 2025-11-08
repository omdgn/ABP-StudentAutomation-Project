using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Attendances;

public interface IAttendanceRepository : IRepository<Attendance, Guid>
{
    Task<List<Attendance>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        CancellationToken cancellationToken = default);

    Task<List<AttendanceWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<AttendanceWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Attendance>> GetAttendancesByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<List<Attendance>> GetAttendancesByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<Attendance?> FindByStudentCourseAndDateAsync(
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        CancellationToken cancellationToken = default);

    Task<int> GetAbsenceCountByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<double> GetAttendanceRateByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        bool? isPresent = null,
        CancellationToken cancellationToken = default);
}
