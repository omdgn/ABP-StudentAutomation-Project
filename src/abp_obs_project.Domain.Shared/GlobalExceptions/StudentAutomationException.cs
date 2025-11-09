using System.Diagnostics.CodeAnalysis;
using Volo.Abp;

namespace abp_obs_project.GlobalExceptions;

/// <summary>
/// Custom exception handler for Student Automation system
/// Provides domain-specific error codes and messages
/// </summary>
public class StudentAutomationException : IStudentAutomationException
{
    private const string DEFAULT_ERROR_MESSAGE = "An unexpected error occurred in Student Automation System.";
    private const string DEFAULT_ERROR_CODE = "SA-10000";

    /// <summary>
    /// Throws exception if condition is true
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="code">Error code (e.g., STD-001, TCH-001)</param>
    /// <param name="condition">Condition to check</param>
    public static void ThrowIf(string? message, string? code, [DoesNotReturnIf(true)] bool condition = true)
    {
        if (condition)
            ThrowException(message, code);
    }

    [DoesNotReturn]
    private static void ThrowException(string? message, string? code)
        => throw new UserFriendlyException(message ?? DEFAULT_ERROR_MESSAGE, code ?? DEFAULT_ERROR_CODE);
}
