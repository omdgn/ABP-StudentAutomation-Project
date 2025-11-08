using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Students;

/// <summary>
/// Student Application Service
/// Inherits from ICrudAppService for standard CRUD operations (ABP best practice)
/// </summary>
public interface IStudentAppService
    : ICrudAppService<StudentDto, Guid, GetStudentsInput, CreateUpdateStudentDto, CreateUpdateStudentDto>
{
    // Additional custom methods can be added here
    // Example: Task<decimal> GetStudentAverageGradeAsync(Guid studentId);
}
