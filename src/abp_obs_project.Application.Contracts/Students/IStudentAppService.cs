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

    /// <summary>
    /// Gets the Student associated with the currently logged-in user.
    /// Returns null if no matching student record exists.
    /// </summary>
    Task<StudentDto?> GetMeAsync();

    /// <summary>
    /// Updates the profile of the currently logged-in student.
    /// Also syncs basic fields to the Identity user (name, surname, phone).
    /// </summary>
    Task<StudentDto> UpdateMyProfileAsync(UpdateMyProfileDto input);
}
