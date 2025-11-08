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
}
