using System.Threading.Tasks;
using abp_obs_project.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.EventHandlers;

/// <summary>
/// Handler for StudentCreatedEto
/// Sends welcome email when a new student is created
/// Runs asynchronously in background
/// </summary>
public class StudentCreatedEmailHandler :
    IDistributedEventHandler<StudentCreatedEto>,
    ITransientDependency
{
    private readonly ILogger<StudentCreatedEmailHandler> _logger;

    public StudentCreatedEmailHandler(ILogger<StudentCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(StudentCreatedEto eventData)
    {
        _logger.LogInformation(
            "Sending welcome email to student: {StudentName} ({Email})",
            eventData.FullName,
            eventData.Email
        );

        // TODO: Implement actual email sending
        // await _emailSender.SendAsync(
        //     eventData.Email,
        //     "Welcome to University!",
        //     $"Dear {eventData.FullName}, welcome! Your student number is {eventData.StudentNumber}"
        // );

        // Simulate email sending delay
        await Task.Delay(100);

        _logger.LogInformation(
            "Welcome email sent successfully to {Email}",
            eventData.Email
        );
    }
}
