using System;
using System.Threading;
using System.Threading.Tasks;
using abp_obs_project.GlobalExceptions;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace abp_obs_project.Students;

/// <summary>
/// Domain service for Student aggregate
/// Handles business logic and validation
/// </summary>
public class StudentManager(IStudentRepository studentRepository) : DomainService
{
    /// <summary>
    /// Creates a new student with validation
    /// </summary>
    public async Task<Student> CreateAsync(
        string firstName,
        string lastName,
        string email,
        string studentNumber,
        DateTime birthDate,
        EnumGender gender = EnumGender.Unknown,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(firstName, lastName, email, studentNumber, birthDate, gender, phoneNumber);

        // Business rule: Email must be unique
        await CheckEmailUniquenessAsync(email, null, cancellationToken);

        // Business rule: Student number must be unique
        await CheckStudentNumberUniquenessAsync(studentNumber, null, cancellationToken);

        // Create student
        var student = new Student(
            GuidGenerator.Create(),
            firstName,
            lastName,
            email,
            studentNumber,
            birthDate,
            gender,
            phoneNumber
        );

        return await studentRepository.InsertAsync(student, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing student with validation
    /// </summary>
    public async Task<Student> UpdateAsync(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string studentNumber,
        DateTime birthDate,
        EnumGender gender = EnumGender.Unknown,
        string? phoneNumber = null,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        CheckValidate(firstName, lastName, email, studentNumber, birthDate, gender, phoneNumber);

        // Get student
        var student = await studentRepository.GetAsync(id, cancellationToken: cancellationToken);
        StudentAutomationException.ThrowIf("Student not found!", "SA-STD-404", student is null);

        // Business rule: Email must be unique
        await CheckEmailUniquenessAsync(email, id, cancellationToken);

        // Business rule: Student number must be unique
        await CheckStudentNumberUniquenessAsync(studentNumber, id, cancellationToken);

        // Update student
        student.SetFirstName(firstName);
        student.SetLastName(lastName);
        student.SetEmail(email);
        student.SetStudentNumber(studentNumber);
        student.SetBirthDate(birthDate);
        student.SetGender(gender);
        student.SetPhoneNumber(phoneNumber);

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            student.ConcurrencyStamp = concurrencyStamp;
        }

        return await studentRepository.UpdateAsync(student, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static void CheckValidate(
        string firstName,
        string lastName,
        string email,
        string studentNumber,
        DateTime birthDate,
        EnumGender gender,
        string? phoneNumber)
    {
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.Length(firstName, nameof(firstName), StudentConsts.MaxFirstNameLength, StudentConsts.MinFirstNameLength);

        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));
        Check.Length(lastName, nameof(lastName), StudentConsts.MaxLastNameLength, StudentConsts.MinLastNameLength);

        Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(email, nameof(email), StudentConsts.MaxEmailLength, StudentConsts.MinEmailLength);

        Check.NotNullOrWhiteSpace(studentNumber, nameof(studentNumber));
        Check.Length(studentNumber, nameof(studentNumber), StudentConsts.MaxStudentNumberLength, StudentConsts.MinStudentNumberLength);

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            Check.Length(phoneNumber, nameof(phoneNumber), StudentConsts.MaxPhoneLength);
        }
    }

    /// <summary>
    /// Checks if email is unique
    /// </summary>
    private async Task CheckEmailUniquenessAsync(string email, Guid? excludeId, CancellationToken cancellationToken)
    {
        var isUnique = await studentRepository.IsEmailUniqueAsync(email, excludeId, cancellationToken);
        StudentAutomationException.ThrowIf(
            $"Email '{email}' is already in use by another student!",
            "SA-STD-002",
            !isUnique
        );
    }

    /// <summary>
    /// Checks if student number is unique
    /// </summary>
    private async Task CheckStudentNumberUniquenessAsync(string studentNumber, Guid? excludeId, CancellationToken cancellationToken)
    {
        var isUnique = await studentRepository.IsStudentNumberUniqueAsync(studentNumber, excludeId, cancellationToken);
        StudentAutomationException.ThrowIf(
            $"Student number '{studentNumber}' is already in use!",
            "SA-STD-003",
            !isUnique
        );
    }
}
