using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Students;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// UI Service implementation for Student operations
/// Inherits all CRUD operations from UIServiceBase
/// </summary>
public class StudentUIService
    : UIServiceBase<IStudentAppService, StudentDto, GetStudentsInput, CreateUpdateStudentDto>,
      IStudentUIService
{
    public StudentUIService(
        IStudentAppService studentAppService,
        ILogger<StudentUIService> logger)
        : base(studentAppService, logger, "Student")
    {
    }

    // Student-specific methods can be added here
}
