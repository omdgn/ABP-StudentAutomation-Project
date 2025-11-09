using System.Threading.Tasks;
using abp_obs_project.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace abp_obs_project.Teachers;

/// <summary>
/// Handles Teacher entity lifecycle events to invalidate cache
/// </summary>
public class TeacherEntityChangedEventHandler :
    ILocalEventHandler<EntityCreatedEventData<Teacher>>,
    ILocalEventHandler<EntityUpdatedEventData<Teacher>>,
    ILocalEventHandler<EntityDeletedEventData<Teacher>>,
    ITransientDependency
{
    private readonly IObsCacheService _cacheService;

    public TeacherEntityChangedEventHandler(IObsCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public virtual async Task HandleEventAsync(EntityCreatedEventData<Teacher> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);
    }

    public virtual async Task HandleEventAsync(EntityUpdatedEventData<Teacher> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);
    }

    public virtual async Task HandleEventAsync(EntityDeletedEventData<Teacher> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Teachers.List);
    }
}
