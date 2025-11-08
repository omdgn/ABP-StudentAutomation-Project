using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Courses;

public interface ICourseRepository : IRepository<Course, Guid>
{
    Task<List<Course>> GetListAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        CancellationToken cancellationToken = default);

    Task<List<CourseWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<CourseWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Course?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> IsCodeUniqueAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<List<Course>> GetCoursesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        CancellationToken cancellationToken = default);
}
