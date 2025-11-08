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
using abp_obs_project.Grades;

namespace abp_obs_project.Repositories;

public class EfCoreGradeRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Grade, Guid>(dbContextProvider), IGradeRepository
{
    public virtual async Task<List<Grade>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, studentId, courseId,
            gradeValueMin, gradeValueMax, status);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? GradeConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, studentId, courseId,
            gradeValueMin, gradeValueMax, status);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<GradeWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, gradeValueMin, gradeValueMax, status);

        var grades = await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? GradeConsts.GetDefaultSorting(true) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return grades.Select(x => new GradeWithNavigationProperties
        {
            Grade = x.Grade,
            Student = x.Student,
            Course = x.Course
        }).ToList();
    }

    public virtual async Task<GradeWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        var result = await query
            .Where(x => x.Grade.Id == id)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

        if (result == null)
        {
            throw new InvalidOperationException($"Grade with id {id} not found");
        }

        return new GradeWithNavigationProperties
        {
            Grade = result.Grade,
            Student = result.Student,
            Course = result.Course
        };
    }

    public virtual async Task<List<Grade>> GetGradesByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(g => g.StudentId == studentId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Grade>> GetGradesByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(g => g.CourseId == courseId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Grade?> FindByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task<double> GetAverageGradeByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        var grades = await query
            .Where(g => g.StudentId == studentId && g.Status == EnumGradeStatus.Passed)
            .Select(g => g.GradeValue)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return grades.Any() ? grades.Average() : 0;
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumGradeStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, null, null, status);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<GradeWithNavigationPropertiesQueryResult>> GetQueryForNavigationPropertiesAsync()
    {
        return from grade in await GetDbSetAsync()
               join student in (await GetDbContextAsync()).Set<Students.Student>() on grade.StudentId equals student.Id
               join course in (await GetDbContextAsync()).Set<Courses.Course>() on grade.CourseId equals course.Id
               select new GradeWithNavigationPropertiesQueryResult
               {
                   Grade = grade,
                   Student = student,
                   Course = course
               };
    }

    protected virtual IQueryable<GradeWithNavigationPropertiesQueryResult> ApplyFilter(
        IQueryable<GradeWithNavigationPropertiesQueryResult> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                e.Student.FirstName.Contains(filterText!) ||
                e.Student.LastName.Contains(filterText!) ||
                e.Course.Name.Contains(filterText!) ||
                (e.Grade.Comments != null && e.Grade.Comments.Contains(filterText!)))
            .WhereIf(studentId.HasValue, e => e.Grade.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.Grade.CourseId == courseId)
            .WhereIf(gradeValueMin.HasValue, e => e.Grade.GradeValue >= gradeValueMin!.Value)
            .WhereIf(gradeValueMax.HasValue, e => e.Grade.GradeValue <= gradeValueMax!.Value)
            .WhereIf(status.HasValue, e => e.Grade.Status == status);
    }

    protected virtual IQueryable<Grade> ApplyFilter(
        IQueryable<Grade> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null)
    {
        return query
            .WhereIf(studentId.HasValue, e => e.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.CourseId == courseId)
            .WhereIf(gradeValueMin.HasValue, e => e.GradeValue >= gradeValueMin!.Value)
            .WhereIf(gradeValueMax.HasValue, e => e.GradeValue <= gradeValueMax!.Value)
            .WhereIf(status.HasValue, e => e.Status == status);
    }

    protected class GradeWithNavigationPropertiesQueryResult
    {
        public Grade Grade { get; set; } = null!;
        public Students.Student Student { get; set; } = null!;
        public Courses.Course Course { get; set; } = null!;
    }
}
