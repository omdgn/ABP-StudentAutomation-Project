using System.Collections.Generic;
using System.Threading.Tasks;
using abp_obs_project.Teachers;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// UI Service for Teacher operations
/// Provides abstraction layer between Blazor components and AppService
/// </summary>
public interface ITeacherUIService
    : IUIServiceBase<TeacherDto, GetTeachersInput, CreateUpdateTeacherDto>
{
    /// <summary>
    /// Gets teacher lookup list for dropdowns
    /// </summary>
    Task<List<TeacherLookupDto>> GetTeacherLookupAsync();
}
