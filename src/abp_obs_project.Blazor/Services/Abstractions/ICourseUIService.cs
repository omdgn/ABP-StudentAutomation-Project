using abp_obs_project.Courses;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// UI Service for Course operations
/// Provides abstraction layer between Blazor components and AppService
/// </summary>
public interface ICourseUIService
    : IUIServiceBase<CourseDto, GetCoursesInput, CreateUpdateCourseDto>
{
    // Course-specific methods can be added here if needed
    // Example: Task<List<StudentDto>> GetCourseStudentsAsync(Guid courseId);
}
