using System.Collections.Generic;
using System.Threading.Tasks;
using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Teachers;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// UI Service implementation for Teacher operations
/// Inherits all CRUD operations from UIServiceBase
/// </summary>
public class TeacherUIService
    : UIServiceBase<ITeacherAppService, TeacherDto, GetTeachersInput, CreateUpdateTeacherDto>,
      ITeacherUIService
{
    public TeacherUIService(
        ITeacherAppService teacherAppService,
        ILogger<TeacherUIService> logger)
        : base(teacherAppService, logger, "Teacher")
    {
    }

    public async Task<List<TeacherLookupDto>> GetTeacherLookupAsync()
    {
        try
        {
            Logger.LogDebug("Fetching teacher lookup list");
            var result = await AppService.GetTeacherLookupAsync();
            Logger.LogDebug("Successfully fetched {Count} teachers for lookup", result.Count);
            return result;
        }
        catch (System.Exception ex)
        {
            Logger.LogError(ex, "Error fetching teacher lookup list");
            throw;
        }
    }
}
