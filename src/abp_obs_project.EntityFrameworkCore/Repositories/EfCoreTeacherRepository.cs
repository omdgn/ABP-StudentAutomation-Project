using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using abp_obs_project.EntityFrameworkCore;
using abp_obs_project.Teachers;

namespace abp_obs_project.Repositories;

public class EfCoreTeacherRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Teacher, Guid>(dbContextProvider), ITeacherRepository
{
    public virtual async Task<List<Teacher>> GetListAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, firstName, lastName, email, department);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? TeacherConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, firstName, lastName, email, department);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Teacher?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(t => t.Email == email, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsEmailUniqueAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(t => t.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return !await query.AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, firstName, lastName, email, department);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual IQueryable<Teacher> ApplyFilter(
        IQueryable<Teacher> query,
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? department = null)
    {
        return query
            // Turkish character friendly search using ILIKE (PostgreSQL case-insensitive)
            // "yılmaz" will match "Yılmaz", "YILMAZ", "yılmaz", etc.
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.FirstName, "default"),
                    EF.Functions.Collate($"%{filterText}%", "default")) ||
                EF.Functions.ILike(EF.Functions.Collate(e.LastName, "default"),
                    EF.Functions.Collate($"%{filterText}%", "default")) ||
                EF.Functions.ILike(e.Email, $"%{filterText}%") ||
                (e.Department != null && EF.Functions.ILike(EF.Functions.Collate(e.Department, "default"),
                    EF.Functions.Collate($"%{filterText}%", "default"))))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.FirstName, "default"),
                    EF.Functions.Collate($"%{firstName}%", "default")))
            .WhereIf(!string.IsNullOrWhiteSpace(lastName), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.LastName, "default"),
                    EF.Functions.Collate($"%{lastName}%", "default")))
            .WhereIf(!string.IsNullOrWhiteSpace(email), e => EF.Functions.ILike(e.Email, $"%{email}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(department), e =>
                e.Department != null && EF.Functions.ILike(EF.Functions.Collate(e.Department, "default"),
                    EF.Functions.Collate($"%{department}%", "default")));
    }
}
