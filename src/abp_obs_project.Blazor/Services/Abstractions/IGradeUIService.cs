using abp_obs_project.Grades;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// UI Service for Grade operations
/// Provides abstraction layer between Blazor components and AppService
/// </summary>
public interface IGradeUIService
    : IUIServiceBase<GradeDto, GetGradesInput, CreateUpdateGradeDto>
{
    // Grade-specific methods can be added here if needed
    // Example: Task<decimal> GetStudentAverageGradeAsync(Guid studentId);
}
