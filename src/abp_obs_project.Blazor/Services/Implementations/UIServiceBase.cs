using System;
using System.Threading.Tasks;
using abp_obs_project.Blazor.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace abp_obs_project.Blazor.Services.Implementations;

/// <summary>
/// Base implementation for all UI Services in Student Automation System
/// Provides centralized:
/// - Error handling
/// - Logging
/// - AppService communication
/// </summary>
public abstract class UIServiceBase<TAppService, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : IUIServiceBase<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>,
      ITransientDependency
    where TAppService : ICrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntityDto : IEntityDto<TKey>
    where TGetListInput : PagedAndSortedResultRequestDto
{
    protected readonly TAppService AppService;
    protected readonly ILogger Logger;
    protected readonly string EntityName;

    protected UIServiceBase(
        TAppService appService,
        ILogger logger,
        string entityName)
    {
        AppService = appService;
        Logger = logger;
        EntityName = entityName;
    }

    public virtual async Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input)
    {
        try
        {
            Logger.LogDebug("Fetching {EntityName} list", EntityName);
            var result = await AppService.GetListAsync(input);
            Logger.LogDebug("Successfully fetched {Count} {EntityName} items", result.Items.Count, EntityName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching {EntityName} list", EntityName);
            throw;
        }
    }

    public virtual async Task<TEntityDto> GetAsync(TKey id)
    {
        try
        {
            Logger.LogDebug("Fetching {EntityName} with ID: {Id}", EntityName, id);
            var result = await AppService.GetAsync(id);
            Logger.LogDebug("Successfully fetched {EntityName}: {Id}", EntityName, id);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching {EntityName} with ID: {Id}", EntityName, id);
            throw;
        }
    }

    public virtual async Task<TEntityDto> CreateAsync(TCreateInput input)
    {
        try
        {
            Logger.LogInformation("Creating new {EntityName}", EntityName);
            var result = await AppService.CreateAsync(input);
            Logger.LogInformation("Successfully created {EntityName} with ID: {Id}", EntityName, result.Id);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {EntityName}", EntityName);
            throw;
        }
    }

    public virtual async Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        try
        {
            Logger.LogInformation("Updating {EntityName}: {Id}", EntityName, id);
            var result = await AppService.UpdateAsync(id, input);
            Logger.LogInformation("Successfully updated {EntityName}: {Id}", EntityName, id);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating {EntityName}: {Id}", EntityName, id);
            throw;
        }
    }

    public virtual async Task DeleteAsync(TKey id)
    {
        try
        {
            Logger.LogWarning("Deleting {EntityName}: {Id}", EntityName, id);
            await AppService.DeleteAsync(id);
            Logger.LogInformation("Successfully deleted {EntityName}: {Id}", EntityName, id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {EntityName}: {Id}", EntityName, id);
            throw;
        }
    }
}

/// <summary>
/// Simplified base class with Guid key and same Create/Update input
/// </summary>
public abstract class UIServiceBase<TAppService, TEntityDto, TGetListInput, TCreateUpdateInput>
    : UIServiceBase<TAppService, TEntityDto, Guid, TGetListInput, TCreateUpdateInput, TCreateUpdateInput>
    where TAppService : ICrudAppService<TEntityDto, Guid, TGetListInput, TCreateUpdateInput, TCreateUpdateInput>
    where TEntityDto : IEntityDto<Guid>
    where TGetListInput : PagedAndSortedResultRequestDto
{
    protected UIServiceBase(
        TAppService appService,
        ILogger logger,
        string entityName)
        : base(appService, logger, entityName)
    {
    }
}
