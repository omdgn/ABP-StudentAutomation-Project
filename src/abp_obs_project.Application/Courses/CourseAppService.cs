using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Permissions;
using abp_obs_project.Teachers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace abp_obs_project.Courses;

/// <summary>
/// Application service for Course operations
/// Orchestrates business logic between UI and Domain layers
/// Demonstrates navigation property mapping (Course -> Teacher)
/// </summary>
[Authorize(abp_obs_projectPermissions.Courses.Default)]
public class CourseAppService : ApplicationService, ICourseAppService
{
    private readonly ICourseRepository _courseRepository;
    private readonly CourseManager _courseManager;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IObsCacheService _cacheService;

    public CourseAppService(
        ICourseRepository courseRepository,
        CourseManager courseManager,
        ITeacherRepository teacherRepository,
        IObsCacheService cacheService)
    {
        _courseRepository = courseRepository;
        _courseManager = courseManager;
        _teacherRepository = teacherRepository;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Gets a paginated and filtered list of courses with teacher information
    /// Uses GetListWithNavigationPropertiesAsync for efficient loading
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.ViewAll)]
    public virtual async Task<PagedResultDto<CourseDto>> GetListAsync(GetCoursesInput input)
    {
        // Use cache only for simple list requests (no filters, no pagination)
        var isSimpleListRequest = string.IsNullOrWhiteSpace(input.FilterText) &&
                                  string.IsNullOrWhiteSpace(input.Name) &&
                                  string.IsNullOrWhiteSpace(input.Code) &&
                                  input.CreditsMin == null &&
                                  input.CreditsMax == null &&
                                  input.Status == null &&
                                  input.TeacherId == null &&
                                  input.SkipCount == 0;

        if (isSimpleListRequest)
        {
            var cachedResult = await _cacheService.GetOrSetAsync(
                ObsCacheKeys.Courses.List,
                async () =>
                {
                    var totalCount = await _courseRepository.GetCountAsync();
                    var items = await _courseRepository.GetListWithNavigationPropertiesAsync(
                        sorting: input.Sorting,
                        maxResultCount: input.MaxResultCount
                    );

                    return new PagedResultDto<CourseDto>
                    {
                        TotalCount = totalCount,
                        Items = items.Select(item =>
                        {
                            var dto = ObjectMapper.Map<Course, CourseDto>(item.Course);
                            dto.TeacherName = $"{item.Teacher.FirstName} {item.Teacher.LastName}";
                            return dto;
                        }).ToList()
                    };
                }
            );

            return cachedResult!;
        }

        // For filtered/paginated requests, bypass cache
        var totalCount = await _courseRepository.GetCountAsync(
            input.FilterText,
            input.Name,
            input.Code,
            input.CreditsMin,
            input.CreditsMax,
            input.Status,
            input.TeacherId
        );

        var items = await _courseRepository.GetListWithNavigationPropertiesAsync(
            input.FilterText,
            input.Name,
            input.Code,
            input.CreditsMin,
            input.CreditsMax,
            input.Status,
            input.TeacherId,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        // Map courses with teacher information
        return new PagedResultDto<CourseDto>
        {
            TotalCount = totalCount,
            Items = items.Select(item =>
            {
                var dto = ObjectMapper.Map<Course, CourseDto>(item.Course);
                dto.TeacherName = $"{item.Teacher.FirstName} {item.Teacher.LastName}";
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// Gets a single course by ID with teacher information
    /// </summary>
    public virtual async Task<CourseDto> GetAsync(Guid id)
    {
        var courseWithNavigation = await _courseRepository.GetWithNavigationPropertiesAsync(id);

        var dto = ObjectMapper.Map<Course, CourseDto>(courseWithNavigation.Course);
        dto.TeacherName = $"{courseWithNavigation.Teacher.FirstName} {courseWithNavigation.Teacher.LastName}";

        return dto;
    }

    /// <summary>
    /// Creates a new course using domain manager for business rules
    /// CourseManager validates teacher existence
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Create)]
    public virtual async Task<CourseDto> CreateAsync(CreateUpdateCourseDto input)
    {
        var course = await _courseManager.CreateAsync(
            input.Name,
            input.Code,
            input.Credits,
            input.TeacherId,
            input.Description,
            input.Status
        );

        // Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);

        return await MapCourseToDtoAsync(course);
    }

    /// <summary>
    /// Updates an existing course using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Edit)]
    public virtual async Task<CourseDto> UpdateAsync(Guid id, CreateUpdateCourseDto input)
    {
        var course = await _courseManager.UpdateAsync(
            id,
            input.Name,
            input.Code,
            input.Credits,
            input.TeacherId,
            input.Description,
            input.Status
        );

        // Invalidate cache after update
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);

        return await MapCourseToDtoAsync(course);
    }

    /// <summary>
    /// Deletes a course
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Courses.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _courseRepository.DeleteAsync(id);

        // Invalidate cache after deletion
        await _cacheService.RemoveAsync(ObsCacheKeys.Courses.List);
    }

    /// <summary>
    /// Helper method to map course to DTO with teacher information
    /// </summary>
    private async Task<CourseDto> MapCourseToDtoAsync(Course course)
    {
        var dto = ObjectMapper.Map<Course, CourseDto>(course);

        var teacher = await _teacherRepository.GetAsync(course.TeacherId);
        dto.TeacherName = $"{teacher.FirstName} {teacher.LastName}";

        return dto;
    }
}
