using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Grades;

/// <summary>
/// Grade Application Service
/// Inherits from ICrudAppService for standard CRUD operations (ABP best practice)
/// </summary>
public interface IGradeAppService
    : ICrudAppService<GradeDto, Guid, GetGradesInput, CreateUpdateGradeDto, CreateUpdateGradeDto>
{
    /// <summary>
    /// Gets average grade for a specific student
    /// </summary>
    Task<double> GetAverageGradeByStudentAsync(Guid studentId);

    /// <summary>
    /// Lists grades for the currently logged-in student.
    /// </summary>
    Task<ListResultDto<GradeDto>> GetMyGradesAsync();
}
