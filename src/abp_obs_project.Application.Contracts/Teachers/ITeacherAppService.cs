using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Teachers;

/// <summary>
/// Teacher Application Service
/// Inherits from ICrudAppService for standard CRUD operations (ABP best practice)
/// </summary>
public interface ITeacherAppService
    : ICrudAppService<TeacherDto, Guid, GetTeachersInput, CreateUpdateTeacherDto, CreateUpdateTeacherDto>
{
    /// <summary>
    /// Gets a lightweight list of teachers for lookups (e.g., dropdowns)
    /// </summary>
    Task<List<TeacherLookupDto>> GetTeacherLookupAsync();

    /// <summary>
    /// Creates a new teacher along with their identity user account for authentication
    /// </summary>
    Task<TeacherDto> CreateTeacherWithUserAsync(CreateTeacherWithUserDto input);

    /// <summary>
    /// Gets the Teacher associated with the current user (by email).
    /// </summary>
    Task<TeacherDto?> GetMeAsync();

    /// <summary>
    /// Updates the profile of the current teacher and syncs Identity user basic info.
    /// </summary>
    Task<TeacherDto> UpdateMyProfileAsync(UpdateMyTeacherProfileDto input);
}
