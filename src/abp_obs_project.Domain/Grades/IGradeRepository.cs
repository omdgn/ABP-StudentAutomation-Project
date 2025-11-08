using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace abp_obs_project.Grades;

public interface IGradeRepository : IRepository<Grade, Guid>
{
    Task<List<Grade>> GetListAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<List<GradeWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        double? gradeValueMin = null,
        double? gradeValueMax = null,
        EnumGradeStatus? status = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<GradeWithNavigationProperties> GetWithNavigationPropertiesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Grade>> GetGradesByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<List<Grade>> GetGradesByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<Grade?> FindByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<double> GetAverageGradeByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(
        string? filterText = null,
        Guid? studentId = null,
        Guid? courseId = null,
        EnumGradeStatus? status = null,
        CancellationToken cancellationToken = default);
}
