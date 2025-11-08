using System;
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
    // Additional custom methods can be added here
}
