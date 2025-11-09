using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Enrollments;

public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
{
    Task<List<Enrollment>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        CancellationToken cancellationToken = default);

    Task<List<EnrollmentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<EnrollmentWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(
        Guid studentId,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(
        Guid courseId,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Enrollment?> FindByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<bool> IsStudentEnrolledAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default);
}
