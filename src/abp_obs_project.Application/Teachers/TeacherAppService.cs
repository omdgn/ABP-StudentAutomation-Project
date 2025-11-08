using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace abp_obs_project.Teachers;

/// <summary>
/// Application service for Teacher operations
/// Orchestrates business logic between UI and Domain layers
/// </summary>
[Authorize(abp_obs_projectPermissions.Teachers.Default)]
public class TeacherAppService : ApplicationService, ITeacherAppService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly TeacherManager _teacherManager;
    private readonly IObsCacheService _cacheService;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IIdentityRoleRepository _identityRoleRepository;

    public TeacherAppService(
        ITeacherRepository teacherRepository,
        TeacherManager teacherManager,
        IObsCacheService cacheService,
        IdentityUserManager identityUserManager,
        IIdentityRoleRepository identityRoleRepository)
    {
        _teacherRepository = teacherRepository;
        _teacherManager = teacherManager;
        _cacheService = cacheService;
        _identityUserManager = identityUserManager;
        _identityRoleRepository = identityRoleRepository;
    }

    /// <summary>
    /// Gets a paginated and filtered list of teachers
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Teachers.ViewAll)]
    public virtual async Task<PagedResultDto<TeacherDto>> GetListAsync(GetTeachersInput input)
    {
        // Use cache only for simple list requests (no filters). Cache full list and paginate in-memory
        var isSimpleListRequest = string.IsNullOrWhiteSpace(input.FilterText) &&
                                  string.IsNullOrWhiteSpace(input.FirstName) &&
                                  string.IsNullOrWhiteSpace(input.LastName) &&
                                  string.IsNullOrWhiteSpace(input.Email) &&
                                  string.IsNullOrWhiteSpace(input.Department);

        if (isSimpleListRequest)
        {
            var cachedResult = await _cacheService.GetOrSetAsync(
                ObsCacheKeys.Teachers.List,
                async () =>
                {
                    var totalCount = await _teacherRepository.GetCountAsync();
                    // Cache all items (no pagination limit)
                    var items = await _teacherRepository.GetListAsync(
                        sorting: input.Sorting,
                        maxResultCount: int.MaxValue
                    );

                    return new PagedResultDto<TeacherDto>
                    {
                        TotalCount = totalCount,
                        Items = ObjectMapper.Map<List<Teacher>, List<TeacherDto>>(items)
                    };
                }
            );

            // Apply pagination to cached result
            var pagedItems = cachedResult!.Items
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<TeacherDto>
            {
                TotalCount = cachedResult.TotalCount,
                Items = pagedItems
            };
        }

        // For filtered/paginated requests, bypass cache
        var totalCount = await _teacherRepository.GetCountAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.Department
        );

        var items = await _teacherRepository.GetListAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.Department,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        return new PagedResultDto<TeacherDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<Teacher>, List<TeacherDto>>(items)
        };
    }

    /// <summary>
    /// Gets a single teacher by ID
    /// </summary>
    public virtual async Task<TeacherDto> GetAsync(Guid id)
    {
        var teacher = await _teacherRepository.GetAsync(id);
        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    /// <summary>
    /// Creates a new teacher using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Teachers.Create)]
    public virtual async Task<TeacherDto> CreateAsync(CreateUpdateTeacherDto input)
    {
        var teacher = await _teacherManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.Department,
            input.PhoneNumber
        );

        // Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    /// <summary>
    /// Updates an existing teacher using domain manager for business rules
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Teachers.Edit)]
    public virtual async Task<TeacherDto> UpdateAsync(Guid id, CreateUpdateTeacherDto input)
    {
        var teacher = await _teacherManager.UpdateAsync(
            id,
            input.FirstName,
            input.LastName,
            input.Email,
            input.Department,
            input.PhoneNumber
        );

        // Invalidate cache after update
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    /// <summary>
    /// Deletes a teacher
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Teachers.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _teacherRepository.DeleteAsync(id);

        // Invalidate cache after deletion
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);
    }

    /// <summary>
    /// Gets a lightweight list of teachers for lookups (e.g., dropdowns)
    /// Uses Select projection to minimize data transfer
    /// </summary>
    public virtual async Task<List<TeacherLookupDto>> GetTeacherLookupAsync()
    {
        var teachers = await _teacherRepository.GetListAsync(
            sorting: TeacherConsts.GetDefaultSorting(false)
        );

        return ObjectMapper.Map<List<Teacher>, List<TeacherLookupDto>>(teachers);
    }

    /// <summary>
    /// Creates a new teacher along with their identity user account for authentication
    /// This method ensures both the teacher entity and identity user are created in a single transaction
    /// </summary>
    [Authorize(abp_obs_projectPermissions.Teachers.Create)]
    [UnitOfWork]
    public virtual async Task<TeacherDto> CreateTeacherWithUserAsync(CreateTeacherWithUserDto input)
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

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            identityUser.SetPhoneNumber(input.PhoneNumber, false);
        }

        // Create user with password
        var identityResult = await _identityUserManager.CreateAsync(identityUser, input.Password);
        if (!identityResult.Succeeded)
        {
            throw new Volo.Abp.BusinessException("TeacherCreation:IdentityUserCreationFailed")
                .WithData("errors", string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        // Step 2: Assign "Teacher" role to the user
        var teacherRole = await _identityRoleRepository.FindByNormalizedNameAsync("TEACHER");
        if (teacherRole != null)
        {
            await _identityUserManager.AddToRoleAsync(identityUser, teacherRole.Name);
        }

        // Step 3: Create Teacher domain entity
        Teacher teacher;
        try
        {
            teacher = await _teacherManager.CreateAsync(
                input.FirstName,
                input.LastName,
                input.Email,
                input.Department,
                input.PhoneNumber
            );
        }
        catch (Exception)
        {
            // If teacher creation fails, delete the identity user to maintain consistency
            await _identityUserManager.DeleteAsync(identityUser);
            throw;
        }

        // Step 4: Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }
}
