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
using abp_obs_project.Students;

namespace abp_obs_project.Repositories;

public class EfCoreStudentRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Student, Guid>(dbContextProvider), IStudentRepository
{
    public virtual async Task<List<Student>> GetListAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, firstName, lastName, email,
            studentNumber, gender, birthDateMin, birthDateMax);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? StudentConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, firstName, lastName, email,
            studentNumber, gender, birthDateMin, birthDateMax);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Student?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(s => s.Email == email, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Student?> FindByStudentNumberAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber,
            GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsEmailUniqueAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(s => s.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsStudentNumberUniqueAsync(
        string studentNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(s => s.StudentNumber == studentNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, firstName, lastName, email, studentNumber, gender, null, null);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual IQueryable<Student> ApplyFilter(
        IQueryable<Student> query,
        string? filterText = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? studentNumber = null,
        EnumGender? gender = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null)
    {
        return query
            // Turkish character friendly search using unaccent + ILIKE
            // "Ayşe" will match "ayse", "AYSE", "Ayşe", etc.
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.FirstName, "default"),
                    EF.Functions.Collate($"%{filterText}%", "default")) ||
                EF.Functions.ILike(EF.Functions.Collate(e.LastName, "default"),
                    EF.Functions.Collate($"%{filterText}%", "default")) ||
                EF.Functions.ILike(e.Email, $"%{filterText}%") ||
                EF.Functions.ILike(e.StudentNumber, $"%{filterText}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.FirstName, "default"),
                    EF.Functions.Collate($"%{firstName}%", "default")))
            .WhereIf(!string.IsNullOrWhiteSpace(lastName), e =>
                EF.Functions.ILike(EF.Functions.Collate(e.LastName, "default"),
                    EF.Functions.Collate($"%{lastName}%", "default")))
            .WhereIf(!string.IsNullOrWhiteSpace(email), e => EF.Functions.ILike(e.Email, $"%{email}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(studentNumber), e => EF.Functions.ILike(e.StudentNumber, $"%{studentNumber}%"))
            .WhereIf(gender.HasValue, e => e.Gender == gender)
            .WhereIf(birthDateMin.HasValue, e => e.BirthDate >= birthDateMin!.Value)
            .WhereIf(birthDateMax.HasValue, e => e.BirthDate <= birthDateMax!.Value);
    }
}
