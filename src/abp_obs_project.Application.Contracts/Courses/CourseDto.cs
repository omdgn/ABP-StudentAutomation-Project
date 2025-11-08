using System;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Courses;

/// <summary>
/// Course data transfer object with teacher information
/// </summary>
public class CourseDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
    public EnumCourseStatus Status { get; set; }
    public Guid TeacherId { get; set; }

    // Teacher navigation property for display purposes
    public string TeacherName { get; set; } = string.Empty;
}
