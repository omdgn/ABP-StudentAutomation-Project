using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Grades;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// UI Service implementation for Grade operations
/// Inherits all CRUD operations from UIServiceBase
/// </summary>
public class GradeUIService
    : UIServiceBase<IGradeAppService, GradeDto, GetGradesInput, CreateUpdateGradeDto>,
      IGradeUIService
{
    public GradeUIService(
        IGradeAppService gradeAppService,
        ILogger<GradeUIService> logger)
        : base(gradeAppService, logger, "Grade")
    {
    }

    // Grade-specific methods can be added here
}
