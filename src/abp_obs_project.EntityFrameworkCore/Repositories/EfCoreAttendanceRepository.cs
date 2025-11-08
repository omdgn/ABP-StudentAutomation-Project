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
using abp_obs_project.Attendances;

namespace abp_obs_project.Repositories;

public class EfCoreAttendanceRepository(IDbContextProvider<abp_obs_projectDbContext> dbContextProvider)
    : EfCoreRepository<abp_obs_projectDbContext, Attendance, Guid>(dbContextProvider), IAttendanceRepository
{
    public virtual async Task<List<Attendance>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetQueryableAsync(), filterText, studentId, courseId,
            attendanceDateMin, attendanceDateMax, isPresent);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? AttendanceConsts.GetDefaultSorting(false) : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(await GetDbSetAsync(), filterText, studentId, courseId,
            attendanceDateMin, attendanceDateMax, isPresent);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<AttendanceWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, attendanceDateMin, attendanceDateMax, isPresent);

        var attendances = await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? AttendanceConsts.GetDefaultSorting(true) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return attendances.Select(x => new AttendanceWithNavigationProperties
        {
            Attendance = x.Attendance,
            Student = x.Student,
            Course = x.Course
        }).ToList();
    }

    public virtual async Task<AttendanceWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryForNavigationPropertiesAsync();
        var result = await query
            .Where(x => x.Attendance.Id == id)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

        if (result == null)
        {
            throw new InvalidOperationException($"Attendance with id {id} not found");
        }

        return new AttendanceWithNavigationProperties
        {
            Attendance = result.Attendance,
            Student = result.Student,
            Course = result.Course
        };
    }

    public virtual async Task<List<Attendance>> GetAttendancesByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(a => a.StudentId == studentId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Attendance>> GetAttendancesByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(a => a.CourseId == courseId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<Attendance?> FindByStudentCourseAndDateAsync(
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .FirstOrDefaultAsync(a =>
                a.StudentId == studentId &&
                a.CourseId == courseId &&
                a.AttendanceDate.Date == attendanceDate.Date,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetAbsenceCountByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(a => a.StudentId == studentId && a.CourseId == courseId && !a.IsPresent)
            .CountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<double> GetAttendanceRateByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        var attendances = await query
            .Where(a => a.StudentId == studentId && a.CourseId == courseId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (!attendances.Any())
        {
            return 0;
        }

        var presentCount = attendances.Count(a => a.IsPresent);
        return (double)presentCount / attendances.Count * 100;
    }

    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        bool? isPresent = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, filterText, studentId, courseId, null, null, isPresent);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<AttendanceWithNavigationPropertiesQueryResult>> GetQueryForNavigationPropertiesAsync()
    {
        return from attendance in await GetDbSetAsync()
               join student in (await GetDbContextAsync()).Set<Students.Student>() on attendance.StudentId equals student.Id
               join course in (await GetDbContextAsync()).Set<Courses.Course>() on attendance.CourseId equals course.Id
               select new AttendanceWithNavigationPropertiesQueryResult
               {
                   Attendance = attendance,
                   Student = student,
                   Course = course
               };
    }

    protected virtual IQueryable<AttendanceWithNavigationPropertiesQueryResult> ApplyFilter(
        IQueryable<AttendanceWithNavigationPropertiesQueryResult> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText), e =>
                e.Student.FirstName.Contains(filterText!) ||
                e.Student.LastName.Contains(filterText!) ||
                e.Course.Name.Contains(filterText!))
            .WhereIf(studentId.HasValue, e => e.Attendance.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.Attendance.CourseId == courseId)
            .WhereIf(attendanceDateMin.HasValue, e => e.Attendance.AttendanceDate >= attendanceDateMin!.Value)
            .WhereIf(attendanceDateMax.HasValue, e => e.Attendance.AttendanceDate <= attendanceDateMax!.Value)
            .WhereIf(isPresent.HasValue, e => e.Attendance.IsPresent == isPresent);
    }

    protected virtual IQueryable<Attendance> ApplyFilter(
        IQueryable<Attendance> query,
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        DateTime? attendanceDateMin = null,
        DateTime? attendanceDateMax = null,
        bool? isPresent = null)
    {
        return query
            .WhereIf(studentId.HasValue, e => e.StudentId == studentId)
            .WhereIf(courseId.HasValue, e => e.CourseId == courseId)
            .WhereIf(attendanceDateMin.HasValue, e => e.AttendanceDate >= attendanceDateMin!.Value)
            .WhereIf(attendanceDateMax.HasValue, e => e.AttendanceDate <= attendanceDateMax!.Value)
            .WhereIf(isPresent.HasValue, e => e.IsPresent == isPresent);
    }

    protected class AttendanceWithNavigationPropertiesQueryResult
    {
        public Attendance Attendance { get; set; } = null!;
        public Students.Student Student { get; set; } = null!;
        public Courses.Course Course { get; set; } = null!;
    }
}
