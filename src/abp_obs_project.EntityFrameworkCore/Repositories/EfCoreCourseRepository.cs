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
using abp_obs_project.Courses;

namespace abp_obs_project.Repositories;

public class EfCoreCourseRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Course, Guid>(dbContextProvider), ICourseRepository
{
    public virtual async Task<List<Course>> GetListAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, name, code, creditsMin, creditsMax,
            status, teacherId);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CourseConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, name, code, creditsMin, creditsMax,
            status, teacherId);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<CourseWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        query = ApplyFilter(query, filterText, name, code, creditsMin, creditsMax, status, teacherId);

        var courses = await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? CourseConsts.GetDefaultSorting(true) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return courses.Select(x => new CourseWithNavigationProperties
        {
            Course = x.Course,
            Teacher = x.Teacher
        }).ToList();
    }

    public virtual async Task<CourseWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        var result = await query
            .Where(x => x.Course.Id == id)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

        if (result == null)
        {
            throw new InvalidOperationException($"Course with id {id} not found");
        }

        return new CourseWithNavigationProperties
        {
            Course = result.Course,
            Teacher = result.Teacher
        };
    }

    public virtual async Task<Course?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(c => c.Code == code, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsCodeUniqueAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(c => c.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Course>> GetCoursesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        string? name = null,
        string? code = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, name, code, null, null, status, teacherId);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<CourseWithNavigationPropertiesQueryResult>> GetQueryForNavigationPropertiesAsync()
    {
        return from course in await GetDbSetAsync()
               join teacher in (await GetDbContextAsync()).Set<Teachers.Teacher>() on course.TeacherId equals teacher.Id into teachers
               from teacher in teachers.DefaultIfEmpty()
               select new CourseWithNavigationPropertiesQueryResult
               {
                   Course = course,
                   Teacher = teacher
               };
    }

    protected virtual IQueryable<CourseWithNavigationPropertiesQueryResult> ApplyFilter(
        IQueryable<CourseWithNavigationPropertiesQueryResult> query,
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                e.Course.Name.Contains(filterText!) ||
                e.Course.Code.Contains(filterText!))
            .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Course.Name.Contains(name!))
            .WhereIf(!string.IsNullOrWhiteSpace(code), e => e.Course.Code.Contains(code!))
            .WhereIf(creditsMin.HasValue, e => e.Course.Credits >= creditsMin!.Value)
            .WhereIf(creditsMax.HasValue, e => e.Course.Credits <= creditsMax!.Value)
            .WhereIf(status.HasValue, e => e.Course.Status == status)
            .WhereIf(teacherId.HasValue, e => e.Course.TeacherId == teacherId);
    }

    protected virtual IQueryable<Course> ApplyFilter(
        IQueryable<Course> query,
        string? filterText = null,
        string? name = null,
        string? code = null,
        int? creditsMin = null,
        int? creditsMax = null,
        EnumCourseStatus? status = null,
        Guid? teacherId = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                e.Name.Contains(filterText!) ||
                e.Code.Contains(filterText!))
            .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name!))
            .WhereIf(!string.IsNullOrWhiteSpace(code), e => e.Code.Contains(code!))
            .WhereIf(creditsMin.HasValue, e => e.Credits >= creditsMin!.Value)
            .WhereIf(creditsMax.HasValue, e => e.Credits <= creditsMax!.Value)
            .WhereIf(status.HasValue, e => e.Status == status)
            .WhereIf(teacherId.HasValue, e => e.TeacherId == teacherId);
    }

    protected class CourseWithNavigationPropertiesQueryResult
    {
        public Course Course { get; set; } = null!;
        public Teachers.Teacher? Teacher { get; set; }
    }
}
