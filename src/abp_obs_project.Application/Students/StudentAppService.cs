using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Events;
using abp_obs_project.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace abp_obs_project.Students;

[Authorize(abp_obs_projectPermissions.Students.Default)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObsCacheService _cacheService;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IIdentityRoleRepository _identityRoleRepository;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager,
        IDistributedEventBus distributedEventBus,
        IObsCacheService cacheService,
        IdentityUserManager identityUserManager,
        IIdentityRoleRepository identityRoleRepository)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
        _distributedEventBus = distributedEventBus;
        _cacheService = cacheService;
        _identityUserManager = identityUserManager;
        _identityRoleRepository = identityRoleRepository;
    }

    [Authorize(abp_obs_projectPermissions.Students.ViewAll)]
    public virtual async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentsInput input)
    {
        // Use cache only for simple list requests (no filters, no pagination)
        // Cache can be used for simple list requests (no filters)
        // Pagination (SkipCount) will be applied after getting from cache
        var isSimpleListRequest = string.IsNullOrWhiteSpace(input.FilterText) &&
                                  string.IsNullOrWhiteSpace(input.FirstName) &&
                                  string.IsNullOrWhiteSpace(input.LastName) &&
                                  string.IsNullOrWhiteSpace(input.Email) &&
                                  string.IsNullOrWhiteSpace(input.StudentNumber) &&
                                  input.Gender == null &&
                                  input.BirthDateMin == null &&
                                  input.BirthDateMax == null;

        if (isSimpleListRequest)
        {
            var cachedResult = await _cacheService.GetOrSetAsync(
                ObsCacheKeys.Students.List,
                async () =>
                {
                    var totalCount = await _studentRepository.GetCountAsync();
                    // Cache all items (no pagination limit)
                    var items = await _studentRepository.GetListAsync(
                        sorting: input.Sorting,
                        maxResultCount: int.MaxValue  // Get all items for cache
                    );

                    return new PagedResultDto<StudentDto>
                    {
                        TotalCount = totalCount,
                        Items = ObjectMapper.Map<List<Student>, List<StudentDto>>(items)
                    };
                }
            );

            // Apply pagination to cached result
            var pagedItems = cachedResult!.Items
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<StudentDto>
            {
                TotalCount = cachedResult.TotalCount,
                Items = pagedItems
            };
        }

        // For filtered/paginated requests, bypass cache
        var totalCount = await _studentRepository.GetCountAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.Gender,
            input.BirthDateMin,
            input.BirthDateMax
        );

        var items = await _studentRepository.GetListAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.Gender,
            input.BirthDateMin,
            input.BirthDateMax,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        return new PagedResultDto<StudentDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<Student>, List<StudentDto>>(items)
        };
    }

    public virtual async Task<StudentDto> GetAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    /// <summary>
    /// Gets the Student associated with the current user (by email).
    /// Does not require ViewAll; limited to self access.
    /// </summary>
    public virtual async Task<StudentDto?> GetMeAsync()
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var student = await _studentRepository.FindByEmailAsync(email);
        return student == null ? null : ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Create)]
    public virtual async Task<StudentDto> CreateAsync(CreateUpdateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.BirthDate,
            input.Gender,
            input.Phone
        );

        // Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentCreatedEto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentNumber = student.StudentNumber,
            CreationTime = Clock.Now
        });

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    /// <summary>
    /// Updates current student's profile and syncs Identity user basic info.
    /// </summary>
    public virtual async Task<StudentDto> UpdateMyProfileAsync(UpdateMyProfileDto input)
    {
        var email = CurrentUser.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException("Not authenticated");
        }

        // Load student by email
        var student = await _studentRepository.FindByEmailAsync(email);
        if (student == null)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException("Student record not found for current user");
        }

        // Update domain entity
        student.SetFirstName(input.FirstName);
        student.SetLastName(input.LastName);
        student.SetBirthDate(input.BirthDate);
        student.SetGender(input.Gender);
        student.SetPhoneNumber(input.Phone);

        student = await _studentRepository.UpdateAsync(student);

        // Sync Identity user basic info
        var identityUser = await _identityUserManager.FindByEmailAsync(email);
        if (identityUser != null)
        {
            identityUser.Name = input.FirstName;
            identityUser.Surname = input.LastName;
            if (!string.IsNullOrWhiteSpace(input.Phone))
            {
                identityUser.SetPhoneNumber(input.Phone, false);
            }
            await _identityUserManager.UpdateAsync(identityUser);
        }

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    /// <summary>
    /// Creates a new student along with their identity user account for authentication
    /// This method ensures both the student entity and identity user are created in a single transaction
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Students.Create)]
    [UnitOfWork]
    public virtual async Task<StudentDto> CreateStudentWithUserAsync(CreateStudentWithUserDto input)
    {
        // Step 1: Create Identity User for authentication
        var identityUser = new IdentityUser(
            GuidGenerator.Create(),
            input.UserName,
            input.Email,
            CurrentTenant.Id
        );

        identityUser.SetEmailConfirmed(true); // Auto-confirm email for admin-created accounts

        // Set user's name and surname
        identityUser.Name = input.FirstName;
        identityUser.Surname = input.LastName;

        if (!string.IsNullOrWhiteSpace(input.Phone))
        {
            identityUser.SetPhoneNumber(input.Phone, false);
        }

        // Create user with password
        var identityResult = await _identityUserManager.CreateAsync(identityUser, input.Password);
        if (!identityResult.Succeeded)
        {
            throw new Volo.Abp.BusinessException("StudentCreation:IdentityUserCreationFailed")
                .WithData("errors", string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        // Step 2: Assign "Student" role to the user
        var studentRole = await _identityRoleRepository.FindByNormalizedNameAsync("STUDENT");
        if (studentRole != null)
        {
            await _identityUserManager.AddToRoleAsync(identityUser, studentRole.Name);
        }

        // Step 3: Create Student domain entity
        Student student;
        try
        {
            student = await _studentManager.CreateAsync(
                input.FirstName,
                input.LastName,
                input.Email,
                input.StudentNumber,
                input.BirthDate,
                input.Gender,
                input.Phone
            );
        }
        catch (Exception)
        {
            // If student creation fails, delete the identity user to maintain consistency
            await _identityUserManager.DeleteAsync(identityUser);
            throw;
        }

        // Step 4: Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Step 5: Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentCreatedEto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentNumber = student.StudentNumber,
            CreationTime = Clock.Now
        });

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Edit)]
    public virtual async Task<StudentDto> UpdateAsync(Guid id, CreateUpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        await _studentManager.UpdateAsync(
            id,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.BirthDate,
            input.Gender,
            input.Phone
        );

        // Invalidate cache after update
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentUpdatedEto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentNumber = student.StudentNumber,
            LastModificationTime = Clock.Now
        });

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);

        await _studentRepository.DeleteAsync(id);

        // Invalidate cache after deletion
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentDeletedEto
        {
            Id = student.Id,
            StudentNumber = student.StudentNumber,
            DeletionTime = Clock.Now
        });
    }
}
