using System;
using System.Threading.Tasks;
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
    /// <summary>
    /// Creates a new student along with their identity user account for authentication
    /// </summary>
    Task<StudentDto> CreateStudentWithUserAsync(CreateStudentWithUserDto input);
}
