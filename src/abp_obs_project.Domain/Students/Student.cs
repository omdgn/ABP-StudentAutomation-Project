using JetBrains.Annotations;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Students;

/// <summary>
/// Student aggregate root entity
/// Represents a student in the automation system
/// </summary>
public sealed class Student : FullAuditedAggregateRoot<Guid>
{
    [NotNull]
    public string FirstName { get; private set; } = string.Empty;

    [NotNull]
    public string LastName { get; private set; } = string.Empty;

    [NotNull]
    public string Email { get; private set; } = string.Empty;

    [NotNull]
    public string StudentNumber { get; private set; } = string.Empty;

    public DateTime BirthDate { get; private set; }

    [NotNull]
    public EnumGender Gender { get; private set; }

    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    private Student()
    {
    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Student(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string studentNumber,
        DateTime birthDate,
        EnumGender gender = EnumGender.Unknown,
        string? phoneNumber = null) : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetStudentNumber(studentNumber);
        SetBirthDate(birthDate);
        SetGender(gender);
        SetPhoneNumber(phoneNumber);
    }

    // Setter methods with validation
    public void SetFirstName(string firstName)
    {
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.Length(firstName, nameof(firstName), StudentConsts.MaxFirstNameLength, StudentConsts.MinFirstNameLength);
        FirstName = firstName;
    }

    public void SetLastName(string lastName)
    {
        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));
        Check.Length(lastName, nameof(lastName), StudentConsts.MaxLastNameLength, StudentConsts.MinLastNameLength);
        LastName = lastName;
    }

    public void SetEmail(string email)
    {
        Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(email, nameof(email), StudentConsts.MaxEmailLength, StudentConsts.MinEmailLength);
        Email = email;
    }

    public void SetStudentNumber(string studentNumber)
    {
        Check.NotNullOrWhiteSpace(studentNumber, nameof(studentNumber));
        Check.Length(studentNumber, nameof(studentNumber), StudentConsts.MaxStudentNumberLength, StudentConsts.MinStudentNumberLength);
        StudentNumber = studentNumber;
    }

    public void SetBirthDate(DateTime birthDate)
    {
        // Business rule: Student must be at least 10 years old
        var minBirthDate = DateTime.Now.AddYears(-10);
        if (birthDate > minBirthDate)
        {
            throw new BusinessException("SA-STD-001")
                .WithData("MinAge", 10);
        }

        BirthDate = birthDate;
    }

    public void SetGender(EnumGender gender)
    {
        Gender = gender;
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            Check.Length(phoneNumber, nameof(phoneNumber), StudentConsts.MaxPhoneLength);
        }
        PhoneNumber = phoneNumber;
    }
}
