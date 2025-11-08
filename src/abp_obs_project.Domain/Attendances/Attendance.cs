using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Attendances;

/// <summary>
/// Attendance entity (not an aggregate root)
/// Represents student attendance for a course session
/// </summary>
public class Attendance : FullAuditedEntity<Guid>
{
    public Guid StudentId { get; private set; }

    public Guid CourseId { get; private set; }

    public DateTime AttendanceDate { get; private set; }

    public bool IsPresent { get; private set; }

    public string? Remarks { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    protected Attendance()
    {
        
    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Attendance(
        Guid id,
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        bool isPresent = false,
        string? remarks = null) : base(id)
    {
        SetStudentId(studentId);
        SetCourseId(courseId);
        SetAttendanceDate(attendanceDate);
        SetIsPresent(isPresent);
        SetRemarks(remarks);
    }

    // Setter methods with validation
    public void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("SA-ATD-001")
                .WithData("Field", nameof(studentId));
        }
        StudentId = studentId;
    }

    public void SetCourseId(Guid courseId)
    {
        if (courseId == Guid.Empty)
        {
            throw new BusinessException("SA-ATD-002")
                .WithData("Field", nameof(courseId));
        }
        CourseId = courseId;
    }

    public void SetAttendanceDate(DateTime attendanceDate)
    {
        // Business rule: Attendance date cannot be in the future
        if (attendanceDate > DateTime.Now)
        {
            throw new BusinessException("SA-ATD-003")
                .WithData("Message", "Attendance date cannot be in the future");
        }
        AttendanceDate = attendanceDate;
    }

    public void SetIsPresent(bool isPresent)
    {
        IsPresent = isPresent;
    }

    public void SetRemarks(string? remarks)
    {
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            Check.Length(remarks, nameof(remarks), AttendanceConsts.MaxRemarksLength);
        }
        Remarks = remarks;
    }

    /// <summary>
    /// Marks student as present
    /// </summary>
    public void MarkAsPresent(string? remarks = null)
    {
        IsPresent = true;
        SetRemarks(remarks);
    }

    /// <summary>
    /// Marks student as absent
    /// </summary>
    public void MarkAsAbsent(string? remarks = null)
    {
        IsPresent = false;
        SetRemarks(remarks);
    }
}
