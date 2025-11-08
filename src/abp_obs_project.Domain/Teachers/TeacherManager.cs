using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.GlobalExceptions;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Teachers;

/// <summary>
/// Domain service for Teacher aggregate
/// Handles business logic and validation
/// </summary>
public class TeacherManager(ITeacherRepository teacherRepository) : DomainService
{
    /// <summary>
    /// Creates a new teacher with validation
    /// </summary>
    public async Task<Teacher> CreateAsync(
        string firstName,
        string lastName,
        string email,
        string? department = null,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(firstName, lastName, email, department, phoneNumber);

        // Business rule: Email must be unique
        await CheckEmailUniquenessAsync(email, null, cancellationToken);

        // Create teacher
        var teacher = new Teacher(
            GuidGenerator.Create(),
            firstName,
            lastName,
            email,
            department,
            phoneNumber
        );

        return await teacherRepository.InsertAsync(teacher, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing teacher with validation
    /// </summary>
    public async Task<Teacher> UpdateAsync(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string? department = null,
        string? phoneNumber = null,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(firstName, lastName, email, department, phoneNumber);

        // Get teacher
        var teacher = await teacherRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Teacher not found!", "SA-TCH-404", teacher is null);

        // Business rule: Email must be unique
        await CheckEmailUniquenessAsync(email, id, cancellationToken);

        // Update teacher
        teacher.SetFirstName(firstName);
        teacher.SetLastName(lastName);
        teacher.SetEmail(email);
        teacher.SetDepartment(department);
        teacher.SetPhoneNumber(phoneNumber);

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            teacher.ConcurrencyStamp = concurrencyStamp;
        }

        return await teacherRepository.UpdateAsync(teacher, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static void CheckValidate(
        string firstName,
        string lastName,
        string email,
        string? department,
        string? phoneNumber)
    {
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.Length(firstName, nameof(firstName), TeacherConsts.MaxFirstNameLength, TeacherConsts.MinFirstNameLength);

        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));
        Check.Length(lastName, nameof(lastName), TeacherConsts.MaxLastNameLength, TeacherConsts.MinLastNameLength);

        Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(email, nameof(email), TeacherConsts.MaxEmailLength, TeacherConsts.MinEmailLength);

        if (!string.IsNullOrWhiteSpace(department))
        {
            Check.Length(department, nameof(department), TeacherConsts.MaxDepartmentLength);
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            Check.Length(phoneNumber, nameof(phoneNumber), TeacherConsts.MaxPhoneNumberLength);
        }
    }

    /// <summary>
    /// Checks if email is unique
    /// </summary>
    private async Task CheckEmailUniquenessAsync(string email, Guid? excludeId, CancellationToken cancellationToken)
    {
        var isUnique = await teacherRepository.IsEmailUniqueAsync(email, excludeId, cancellationToken);
        StudentAutomationException.ThrowIf(
            $"Email '{email}' is already in use by another teacher!",
            "SA-TCH-002",
            !isUnique
        );
    }
}
