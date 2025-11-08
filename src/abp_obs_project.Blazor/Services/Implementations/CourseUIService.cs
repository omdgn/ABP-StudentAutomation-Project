using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Courses;
using Microsoft.Extensions.Logging;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// UI Service implementation for Course operations
/// Inherits all CRUD operations from UIServiceBase
/// </summary>
public class CourseUIService
    : UIServiceBase<ICourseAppService, CourseDto, GetCoursesInput, CreateUpdateCourseDto>,
      ICourseUIService
{
    public CourseUIService(
        ICourseAppService courseAppService,
        ILogger<CourseUIService> logger)
        : base(courseAppService, logger, "Course")
    {
    }

    // Course-specific methods can be added here
}
