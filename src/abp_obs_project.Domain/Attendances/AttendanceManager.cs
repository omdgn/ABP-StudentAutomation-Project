using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.GlobalExceptions;
using abp_obs_project.Students;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Attendances;

/// <summary>
/// Domain service for Attendance entity
/// Handles business logic and validation
/// </summary>
public class AttendanceManager(
    IAttendanceRepository attendanceRepository,
    IStudentRepository studentRepository,
    ICourseRepository courseRepository) : DomainService
{
    /// <summary>
    /// Creates a new attendance record with validation
    /// </summary>
    public async Task<Attendance> CreateAsync(
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        bool isPresent = false,
        string? remarks = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(attendanceDate, remarks);

        // Business rule: Student must exist
        await CheckStudentExistsAsync(studentId, cancellationToken);

        // Business rule: Course must exist
        await CheckCourseExistsAsync(courseId, cancellationToken);

        // Business rule: Check for duplicate attendance
        await CheckDuplicateAttendanceAsync(studentId, courseId, attendanceDate, null, cancellationToken);

        // Create attendance
        var attendance = new Attendance(
            GuidGenerator.Create(),
            studentId,
            courseId,
            attendanceDate,
            isPresent,
            remarks
        );

        return await attendanceRepository.InsertAsync(attendance, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing attendance record with validation
    /// </summary>
    public async Task<Attendance> UpdateAsync(
        Guid id,
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        bool isPresent = false,
        string? remarks = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(attendanceDate, remarks);

        // Get attendance
        var attendance = await attendanceRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Attendance record not found!", "SA-ATD-404", attendance is null);

        // Business rule: Student must exist
        await CheckStudentExistsAsync(studentId, cancellationToken);

        // Business rule: Course must exist
        await CheckCourseExistsAsync(courseId, cancellationToken);

        // Business rule: Check for duplicate attendance (if student, course, or date changed)
        if (attendance.StudentId != studentId ||
            attendance.CourseId != courseId ||
            attendance.AttendanceDate.Date != attendanceDate.Date)
        {
            await CheckDuplicateAttendanceAsync(studentId, courseId, attendanceDate, id, cancellationToken);
        }

        // Update attendance
        attendance.SetStudentId(studentId);
        attendance.SetCourseId(courseId);
        attendance.SetAttendanceDate(attendanceDate);
        attendance.SetIsPresent(isPresent);
        attendance.SetRemarks(remarks);

        return await attendanceRepository.UpdateAsync(attendance, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static void CheckValidate(DateTime attendanceDate, string? remarks)
    {
        if (attendanceDate > DateTime.Now)
        {
            throw new BusinessException("SA-ATD-003")
                .WithData("Message", "Attendance date cannot be in the future");
        }

        if (!string.IsNullOrWhiteSpace(remarks))
        {
            Check.Length(remarks, nameof(remarks), AttendanceConsts.MaxRemarksLength);
        }
    }

    /// <summary>
    /// Checks if student exists
    /// </summary>
    private async Task CheckStudentExistsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var studentExists = await studentRepository.FindAsync(studentId, cancellationToken: cancellationToken) != null;
        StudentAutomationException.ThrowIf(
            $"Student with ID '{studentId}' does not exist!",
            "SA-ATD-004",
            !studentExists
        );
    }

    /// <summary>
    /// Checks if course exists
    /// </summary>
    private async Task CheckCourseExistsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var courseExists = await courseRepository.FindAsync(courseId, cancellationToken: cancellationToken) != null;
        StudentAutomationException.ThrowIf(
            $"Course with ID '{courseId}' does not exist!",
            "SA-ATD-005",
            !courseExists
        );
    }

    /// <summary>
    /// Checks for duplicate attendance (same student, course, and date)
    /// </summary>
    private async Task CheckDuplicateAttendanceAsync(
        Guid studentId,
        Guid courseId,
        DateTime attendanceDate,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var existingAttendance = await attendanceRepository.FindByStudentCourseAndDateAsync(
            studentId, courseId, attendanceDate, cancellationToken);

        if (existingAttendance != null && existingAttendance.Id != excludeId)
        {
            StudentAutomationException.ThrowIf(
                $"An attendance record for this student, course, and date already exists!",
                "SA-ATD-006",
                true
            );
        }
    }
}
