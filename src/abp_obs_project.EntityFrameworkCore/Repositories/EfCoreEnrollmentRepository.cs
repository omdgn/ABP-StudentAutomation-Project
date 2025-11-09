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
using abp_obs_project.Enrollments;

namespace abp_obs_project.Repositories;

public class EfCoreEnrollmentRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Enrollment, Guid>(dbContextProvider), IEnrollmentRepository
{
    public virtual async Task<List<Enrollment>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, studentId, courseId,
            status, enrolledAtMin, enrolledAtMax);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? EnrollmentConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, studentId, courseId,
            status, enrolledAtMin, enrolledAtMax);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<EnrollmentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, status, enrolledAtMin, enrolledAtMax);

        // Apply default sorting if not provided
        var sortingToUse = string.IsNullOrWhiteSpace(sorting)
            ? EnrollmentConsts.GetDefaultSorting(withEntityName: true)
            : sorting;

        var enrollments = await query
            .OrderBy(sortingToUse)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return enrollments.Select(x => new EnrollmentWithNavigationProperties
        {
            Enrollment = x.Enrollment,
            Student = x.Student,
            Course = x.Course
        }).ToList();
    }

    public virtual async Task<EnrollmentWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        var result = await query
            .Where(x => x.Enrollment.Id == id)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

        if (result == null)
        {
            throw new InvalidOperationException($"Enrollment with id {id} not found");
        }

        return new EnrollmentWithNavigationProperties
        {
            Enrollment = result.Enrollment,
            Student = result.Student,
            Course = result.Course
        };
    }

    public virtual async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(
        Guid studentId,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(e => e.StudentId == studentId);

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(
        Guid courseId,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(e => e.CourseId == courseId);

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Enrollment?> FindByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsStudentEnrolledAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .AnyAsync(e => e.StudentId == studentId &&
                          e.CourseId == courseId &&
                          e.Status == EnumEnrollmentStatus.Active,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, status, null, null);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<EnrollmentWithNavigationPropertiesQueryResult>> GetQueryForNavigationPropertiesAsync()
    {
        return from enrollment in await GetDbSetAsync()
               join student in (await GetDbContextAsync()).Set<Students.Student>() on enrollment.StudentId equals student.Id
               join course in (await GetDbContextAsync()).Set<Courses.Course>() on enrollment.CourseId equals course.Id
               select new EnrollmentWithNavigationPropertiesQueryResult
               {
                   Enrollment = enrollment,
                   Student = student,
                   Course = course
               };
    }

    protected virtual IQueryable<EnrollmentWithNavigationPropertiesQueryResult> ApplyFilter(
        IQueryable<EnrollmentWithNavigationPropertiesQueryResult> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                e.Student.FirstName.Contains(filterText!) ||
                e.Student.LastName.Contains(filterText!) ||
                e.Student.StudentNumber.Contains(filterText!) ||
                e.Course.Name.Contains(filterText!) ||
                e.Course.Code.Contains(filterText!))
            .WhereIf(studentId.HasValue, e => e.Enrollment.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.Enrollment.CourseId == courseId)
            .WhereIf(status.HasValue, e => e.Enrollment.Status == status)
            .WhereIf(enrolledAtMin.HasValue, e => e.Enrollment.EnrolledAt >= enrolledAtMin!.Value)
            .WhereIf(enrolledAtMax.HasValue, e => e.Enrollment.EnrolledAt <= enrolledAtMax!.Value);
    }

    protected virtual IQueryable<Enrollment> ApplyFilter(
        IQueryable<Enrollment> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumEnrollmentStatus? status = null,
        DateTime? enrolledAtMin = null,
        DateTime? enrolledAtMax = null)
    {
        return query
            .WhereIf(studentId.HasValue, e => e.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.CourseId == courseId)
            .WhereIf(status.HasValue, e => e.Status == status)
            .WhereIf(enrolledAtMin.HasValue, e => e.EnrolledAt >= enrolledAtMin!.Value)
            .WhereIf(enrolledAtMax.HasValue, e => e.EnrolledAt <= enrolledAtMax!.Value);
    }

    protected class EnrollmentWithNavigationPropertiesQueryResult
    {
        public Enrollment Enrollment { get; set; } = null!;
        public Students.Student Student { get; set; } = null!;
        public Courses.Course Course { get; set; } = null!;
    }
}
