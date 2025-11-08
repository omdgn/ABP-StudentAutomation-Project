using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Students;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<List<Student>> GetListAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        CancellationToken cancellationToken = default);

    Task<Student?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Student?> FindByStudentNumberAsync(
        string studentNumber,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsStudentNumberUniqueAsync(
        string studentNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        CancellationToken cancellationToken = default);
}
