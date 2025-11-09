using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using abp_obs_project.Students;
using abp_obs_project.Teachers;

namespace abp_obs_project.Identity;

/// <summary>
/// Automatically creates Student/Teacher entities when a user is assigned the corresponding role.
/// This ensures users registered via self-registration can access their dashboards after admin assigns a role.
/// </summary>
public class UserRoleAssignmentHandler :
    ILocalEventHandler<EntityUpdatedEventData<IdentityUser>>,
    ITransientDependency
{
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<UserRoleAssignmentHandler> _logger;

    public UserRoleAssignmentHandler(
        IRepository<Student, Guid> studentRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IIdentityRoleRepository roleRepository,
        IGuidGenerator guidGenerator,
        ILogger<UserRoleAssignmentHandler> logger)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _roleRepository = roleRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityUpdatedEventData<IdentityUser> eventData)
    {
        try
        {
            var user = eventData.Entity;

            // Handle Student role - create or delete entity based on role assignment
            await HandleStudentRoleAsync(user);

            // Handle Teacher role - create or delete entity based on role assignment
            await HandleTeacherRoleAsync(user);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - prevent blocking user updates
            _logger.LogWarning(ex, "Failed to sync Student/Teacher entity for user {UserId}", eventData.Entity.Id);
        }
    }

    private async Task HandleStudentRoleAsync(IdentityUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            return;
        }

        // Check if user has Student role
        var studentRole = await _roleRepository.FindByNormalizedNameAsync("STUDENT");
        if (studentRole == null)
        {
            return;
        }

        var hasStudentRole = user.Roles.Any(r => r.RoleId == studentRole.Id);
        var existingStudent = await _studentRepository.FirstOrDefaultAsync(s => s.Email == user.Email);

        if (hasStudentRole && existingStudent == null)
        {
            // User has Student role but no Student entity - CREATE it
            var studentNumber = $"STU{DateTime.Now:yyyyMMddHHmmss}";
            var birthDate = DateTime.Now.AddYears(-18); // Default birth date

            var student = new Student(
                _guidGenerator.Create(),
                user.Name ?? "Student",
                user.Surname ?? "User",
                user.Email,
                studentNumber,
                birthDate,
                EnumGender.Unknown,
                user.PhoneNumber
            );

            await _studentRepository.InsertAsync(student, autoSave: true);
            _logger.LogInformation("Auto-created Student entity for user {Email}", user.Email);
        }
        else if (!hasStudentRole && existingStudent != null)
        {
            // User no longer has Student role but Student entity exists - DELETE it
            await _studentRepository.DeleteAsync(existingStudent, autoSave: true);
            _logger.LogInformation("Auto-deleted Student entity for user {Email} (role removed)", user.Email);
        }
    }

    private async Task HandleTeacherRoleAsync(IdentityUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            return;
        }

        // Check if user has Teacher role
        var teacherRole = await _roleRepository.FindByNormalizedNameAsync("TEACHER");
        if (teacherRole == null)
        {
            return;
        }

        var hasTeacherRole = user.Roles.Any(r => r.RoleId == teacherRole.Id);
        var existingTeacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Email == user.Email);

        if (hasTeacherRole && existingTeacher == null)
        {
            // User has Teacher role but no Teacher entity - CREATE it
            var teacher = new Teacher(
                _guidGenerator.Create(),
                user.Name ?? "Teacher",
                user.Surname ?? "User",
                user.Email,
                "General", // Default department
                user.PhoneNumber
            );

            await _teacherRepository.InsertAsync(teacher, autoSave: true);
            _logger.LogInformation("Auto-created Teacher entity for user {Email}", user.Email);
        }
        else if (!hasTeacherRole && existingTeacher != null)
        {
            // User no longer has Teacher role but Teacher entity exists - DELETE it
            await _teacherRepository.DeleteAsync(existingTeacher, autoSave: true);
            _logger.LogInformation("Auto-deleted Teacher entity for user {Email} (role removed)", user.Email);
        }
    }
}
