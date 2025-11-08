using JetBrains.Annotations;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace abp_obs_project.Teachers;

/// <summary>
/// Teacher aggregate root entity
/// Represents a teacher in the automation system
/// </summary>
public sealed class Teacher : FullAuditedAggregateRoot<Guid>
{
    [NotNull]
    public string FirstName { get; private set; } = string.Empty;

    [NotNull]
    public string LastName { get; private set; } = string.Empty;

    [NotNull]
    public string Email { get; private set; } = string.Empty;

    public string? Department { get; private set; } 

    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Private parameterless constructor for ORM
    /// </summary>
    private Teacher()
    {
        
    }

    /// <summary>
    /// Public constructor with validation
    /// </summary>
    public Teacher(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string? department = null,
        string? phoneNumber = null) : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetDepartment(department);
        SetPhoneNumber(phoneNumber);
    }

    // Setter methods with validation
    public void SetFirstName(string firstName)
    {
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.Length(firstName, nameof(firstName), TeacherConsts.MaxFirstNameLength, TeacherConsts.MinFirstNameLength);
        FirstName = firstName;
    }

    public void SetLastName(string lastName)
    {
        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));
        Check.Length(lastName, nameof(lastName), TeacherConsts.MaxLastNameLength, TeacherConsts.MinLastNameLength);
        LastName = lastName;
    }

    public void SetEmail(string email)
    {
        Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(email, nameof(email), TeacherConsts.MaxEmailLength, TeacherConsts.MinEmailLength);
        Email = email;
    }

    public void SetDepartment(string? department)
    {
        if (!string.IsNullOrWhiteSpace(department))
        {
            Check.Length(department, nameof(department), TeacherConsts.MaxDepartmentLength);
        }
        Department = department;
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            Check.Length(phoneNumber, nameof(phoneNumber), TeacherConsts.MaxPhoneNumberLength);
        }
        PhoneNumber = phoneNumber;
    }
}
