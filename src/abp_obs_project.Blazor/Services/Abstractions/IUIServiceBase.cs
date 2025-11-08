using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Services.Abstractions;

/// <summary>
/// Base interface for all UI Services in Student Automation System
/// Provides common CRUD operations for entities
/// </summary>
/// <typeparam name="TEntityDto">Entity DTO type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
/// <typeparam name="TGetListInput">Get list input type</typeparam>
/// <typeparam name="TCreateInput">Create input type</typeparam>
/// <typeparam name="TUpdateInput">Update input type</typeparam>
public interface IUIServiceBase<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntityDto : IEntityDto<TKey>
    where TGetListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Get paginated list of entities
    /// </summary>
    Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input);

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<TEntityDto> GetAsync(TKey id);

    /// <summary>
    /// Create new entity
    /// </summary>
    Task<TEntityDto> CreateAsync(TCreateInput input);

    /// <summary>
    /// Update existing entity
    /// </summary>
    Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input);

    /// <summary>
    /// Delete entity
    /// </summary>
    Task DeleteAsync(TKey id);
}

/// <summary>
/// Simplified base interface for UI Services with Guid key and same Create/Update input
/// </summary>
public interface IUIServiceBase<TEntityDto, TGetListInput, TCreateUpdateInput>
    : IUIServiceBase<TEntityDto, Guid, TGetListInput, TCreateUpdateInput, TCreateUpdateInput>
    where TEntityDto : IEntityDto<Guid>
    where TGetListInput : PagedAndSortedResultRequestDto
{
}
