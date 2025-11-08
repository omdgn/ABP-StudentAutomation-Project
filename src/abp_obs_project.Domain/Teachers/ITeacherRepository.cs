using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Teachers;

public interface ITeacherRepository : IRepository<Teacher, Guid>
{
    Task<List<Teacher>> GetListAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        CancellationToken cancellationToken = default);

    Task<Teacher?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        CancellationToken cancellationToken = default);
}
