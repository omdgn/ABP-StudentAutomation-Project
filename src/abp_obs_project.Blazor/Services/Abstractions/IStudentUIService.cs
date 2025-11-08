using abp_obs_project.Students;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// UI Service for Student operations
/// Provides abstraction layer between Blazor components and AppService
/// </summary>
public interface IStudentUIService
    : IUIServiceBase<StudentDto, GetStudentsInput, CreateUpdateStudentDto>
{
    // Student-specific methods can be added here if needed
    // Example: Task<List<GradeDto>> GetStudentGradesAsync(Guid studentId);
}
