using System.Threading.Tasks;
using abp_obs_project.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace abp_obs_project.Students;

/// <summary>
/// Handles Student entity lifecycle events to invalidate cache
/// </summary>
public class StudentEntityChangedEventHandler :
    ILocalEventHandler<EntityCreatedEventData<Student>>,
    ILocalEventHandler<EntityUpdatedEventData<Student>>,
    ILocalEventHandler<EntityDeletedEventData<Student>>,
    ITransientDependency
{
    private readonly IObsCacheService _cacheService;

    public StudentEntityChangedEventHandler(IObsCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public virtual async Task HandleEventAsync(EntityCreatedEventData<Student> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);
    }

    public virtual async Task HandleEventAsync(EntityUpdatedEventData<Student> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);
    }

    public virtual async Task HandleEventAsync(EntityDeletedEventData<Student> eventData)
    {
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);
    }
}
