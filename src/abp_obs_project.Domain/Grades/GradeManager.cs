using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.Courses;
using abp_obs_project.GlobalExceptions;
using abp_obs_project.Students;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Grades;

/// <summary>
/// Domain service for Grade entity
/// Handles business logic and validation
/// </summary>
public class GradeManager(
    IGradeRepository gradeRepository,
    IStudentRepository studentRepository,
    ICourseRepository courseRepository) : DomainService
{
    /// <summary>
    /// Creates a new grade with validation
    /// </summary>
    public async Task<Grade> CreateAsync(
        Guid studentId,
        Guid courseId,
        double gradeValue,
        string? comments = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(gradeValue, comments);

        // Business rule: Student must exist
        await CheckStudentExistsAsync(studentId, cancellationToken);

        // Business rule: Course must exist
        await CheckCourseExistsAsync(courseId, cancellationToken);

        // Business rule: Check for duplicate grade
        await CheckDuplicateGradeAsync(studentId, courseId, null, cancellationToken);

        // Create grade
        var grade = new Grade(
            GuidGenerator.Create(),
            studentId,
            courseId,
            gradeValue,
            comments
        );

        return await gradeRepository.InsertAsync(grade, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing grade with validation
    /// </summary>
    public async Task<Grade> UpdateAsync(
        Guid id,
        Guid studentId,
        Guid courseId,
        double gradeValue,
        string? comments = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(gradeValue, comments);

        // Get grade
        var grade = await gradeRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Grade not found!", "SA-GRD-404", grade is null);

        // Business rule: Student must exist
        await CheckStudentExistsAsync(studentId, cancellationToken);

        // Business rule: Course must exist
        await CheckCourseExistsAsync(courseId, cancellationToken);

        // Business rule: Check for duplicate grade (if student or course changed)
        if (grade.StudentId != studentId || grade.CourseId != courseId)
        {
            await CheckDuplicateGradeAsync(studentId, courseId, id, cancellationToken);
        }

        // Update grade
        grade.SetStudentId(studentId);
        grade.SetCourseId(courseId);
        grade.SetGradeValue(gradeValue);
        grade.SetComments(comments);
        grade.SetGradedAt(DateTime.Now);

        return await gradeRepository.UpdateAsync(grade, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static void CheckValidate(double gradeValue, string? comments)
    {
        if (gradeValue < GradeConsts.MinGradeValue || gradeValue > GradeConsts.MaxGradeValue)
        {
            throw new BusinessException("SA-GRD-003")
                .WithData("MinGrade", GradeConsts.MinGradeValue)
                .WithData("MaxGrade", GradeConsts.MaxGradeValue)
                .WithData("ProvidedGrade", gradeValue);
        }

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Check.Length(comments, nameof(comments), GradeConsts.MaxCommentsLength);
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
            "SA-GRD-004",
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
            "SA-GRD-005",
            !courseExists
        );
    }

    /// <summary>
    /// Checks for duplicate grade (same student and course)
    /// </summary>
    private async Task CheckDuplicateGradeAsync(
        Guid studentId,
        Guid courseId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var existingGrade = await gradeRepository.FindByStudentAndCourseAsync(studentId, courseId, cancellationToken);

        if (existingGrade != null && existingGrade.Id != excludeId)
        {
            StudentAutomationException.ThrowIf(
                $"A grade for this student and course already exists!",
                "SA-GRD-006",
                true
            );
        }
    }
}
