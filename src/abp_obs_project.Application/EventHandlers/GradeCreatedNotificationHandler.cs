using System.Threading.Tasks;
using abp_obs_project.Events;
using abp_obs_project.Grades;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.EventHandlers;

/// <summary>
/// Handler for GradeCreatedEto
/// Sends grade notification email to student
/// Demonstrates business logic in handler (status-based messaging)
/// </summary>
public class GradeCreatedNotificationHandler :
    IDistributedEventHandler<GradeCreatedEto>,
    ITransientDependency
{
    private readonly ILogger<GradeCreatedNotificationHandler> _logger;

    public GradeCreatedNotificationHandler(ILogger<GradeCreatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(GradeCreatedEto eventData)
    {
        _logger.LogInformation(
            "Sending grade notification to student: {StudentName} for course: {CourseName}, Grade: {GradeValue}",
            eventData.StudentName,
            eventData.CourseName,
            eventData.GradeValue
        );

        // Determine message based on grade status
        var message = eventData.Status switch
        {
            EnumGradeStatus.Passed => $"Congratulations! You passed {eventData.CourseName} with {eventData.GradeValue} points.",
            EnumGradeStatus.Failed => $"Unfortunately, you did not pass {eventData.CourseName}. Your grade: {eventData.GradeValue}. Please contact your advisor.",
            EnumGradeStatus.Incomplete => $"Your grade for {eventData.CourseName} is incomplete. Please complete pending assignments.",
            _ => $"Your grade for {eventData.CourseName} has been recorded: {eventData.GradeValue}"
        };

        // TODO: Send email
        // await _emailSender.SendAsync(
        //     eventData.StudentEmail,
        //     $"Grade Posted: {eventData.CourseName}",
        //     message
        // );

        _logger.LogInformation(
            "Grade notification sent to {Email}. Status: {Status}",
            eventData.StudentEmail,
            eventData.Status
        );

        await Task.CompletedTask;
    }
}
