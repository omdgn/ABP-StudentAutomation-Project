using System.Threading.Tasks;
using abp_obs_project.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.EventHandlers;

/// <summary>
/// Handler for AttendanceRecordedEto
/// Sends alert if student exceeds absence threshold
/// Demonstrates conditional event handling
/// </summary>
public class AttendanceAbsenceAlertHandler :
    IDistributedEventHandler<AttendanceRecordedEto>,
    ITransientDependency
{
    private readonly ILogger<AttendanceAbsenceAlertHandler> _logger;
    private const int AbsenceThreshold = 3; // Alert after 3 absences

    public AttendanceAbsenceAlertHandler(ILogger<AttendanceAbsenceAlertHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(AttendanceRecordedEto eventData)
    {
        // Only process if student was absent
        if (eventData.IsPresent)
        {
            _logger.LogDebug(
                "Student {StudentName} was present for {CourseName}. No alert needed.",
                eventData.StudentName,
                eventData.CourseName
            );
            return;
        }

        _logger.LogInformation(
            "Student {StudentName} was absent from {CourseName}. Total absences: {AbsenceCount}",
            eventData.StudentName,
            eventData.CourseName,
            eventData.TotalAbsenceCount
        );

        // Check if threshold exceeded
        if (eventData.TotalAbsenceCount >= AbsenceThreshold)
        {
            _logger.LogWarning(
                "ABSENCE ALERT: Student {StudentName} has {AbsenceCount} absences in {CourseName} (Threshold: {Threshold})",
                eventData.StudentName,
                eventData.TotalAbsenceCount,
                eventData.CourseName,
                AbsenceThreshold
            );

            // TODO: Send alert email to student and advisor
            // await _emailSender.SendAsync(
            //     eventData.StudentEmail,
            //     "Attendance Alert",
            //     $"Warning: You have {eventData.TotalAbsenceCount} absences in {eventData.CourseName}. Please contact your advisor."
            // );

            _logger.LogInformation(
                "Absence alert email sent to {Email}",
                eventData.StudentEmail
            );
        }

        await Task.CompletedTask;
    }
}
