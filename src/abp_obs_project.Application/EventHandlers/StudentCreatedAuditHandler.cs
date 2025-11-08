using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.EventHandlers;

/// <summary>
/// Handler for StudentCreatedEto
/// Logs student creation to audit system
/// Demonstrates multiple handlers for same event
/// </summary>
public class StudentCreatedAuditHandler :
    IDistributedEventHandler<StudentCreatedEto>,
    ITransientDependency
{
    private readonly ILogger<StudentCreatedAuditHandler> _logger;
    private readonly IObsCacheService _cacheService;

    public StudentCreatedAuditHandler(
        ILogger<StudentCreatedAuditHandler> logger,
        IObsCacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task HandleEventAsync(StudentCreatedEto eventData)
    {
        _logger.LogInformation(
            "[AUDIT] Student created: ID={StudentId}, Name={StudentName}, Number={StudentNumber}, Time={CreationTime}",
            eventData.Id,
            eventData.FullName,
            eventData.StudentNumber,
            eventData.CreationTime
        );

        // Invalidate student list cache
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);
        _logger.LogDebug("Cache invalidated for students list after event: StudentCreated");

        // TODO: Write to audit database or external logging system
        // await _auditService.LogAsync(new AuditEntry
        // {
        //     EntityType = "Student",
        //     EntityId = eventData.Id,
        //     Action = "Created",
        //     Timestamp = eventData.CreationTime
        // });

        await Task.CompletedTask;
    }
}
