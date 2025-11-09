using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Courses;

/// <summary>
/// Course Application Service
/// Inherits from ICrudAppService for standard CRUD operations (ABP best practice)
/// </summary>
public interface ICourseAppService
    : ICrudAppService<CourseDto, Guid, GetCoursesInput, CreateUpdateCourseDto, CreateUpdateCourseDto>
{
    /// <summary>
    /// Returns the list of courses that the current student is enrolled in.
    /// </summary>
    Task<ListResultDto<CourseDto>> GetMyCoursesAsync();

    /// <summary>
    /// Returns a paged and filtered list of the current student's courses.
    /// </summary>
    Task<PagedResultDto<CourseDto>> GetMyCoursesAsync(GetMyCoursesInput input);
}
