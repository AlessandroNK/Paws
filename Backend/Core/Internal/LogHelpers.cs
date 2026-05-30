using Backend.Core.Models.Results;

namespace Backend.Core.Internal;

public static class LogHelpers
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static ILogger ResolveLogger(ILogger? logger)
    {
        return logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogResult(ILogger logger, Result result, string message = "")
    {
        // Result<T> inherits Result, so this single overload handles both Result and Result<T>.
        if (result) LogInfo(logger, result, message);
        else LogError(logger, result, message);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(Exception e, string message = "")
    {
        LogError(null, e, message);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(Exception e, string message = "")
    {
        LogInfo(null, e, message);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogWarning(Exception e, string message = "")
    {
        LogWarning(null, e, message);
    }

    #region Error

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger? logger, Result result, string message = "")
    {
        var log = ResolveLogger(logger);
        log.LogError("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        foreach (var line in FormatResultBlock(result, message))
            log.LogError(line);
        log.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger? logger, Exception e, string message = "")
    {
        var log = ResolveLogger(logger);
        log.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrWhiteSpace(message)) log.LogError(message);
        log.LogError(e.Message);
        log.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger? logger, string message)
    {
        var log = ResolveLogger(logger);
        log.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        log.LogError(message);
        log.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Info

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger? logger, Result result, string message = "")
    {
        var log = ResolveLogger(logger);
        log.LogInformation("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> SUCCESS");
        foreach (var line in FormatResultBlock(result, message))
            log.LogInformation(line);
        log.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger? logger, string message)
    {
        var log = ResolveLogger(logger);
        log.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> INFO");
        log.LogInformation(message);
        log.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger? logger, Exception e, string message = "")
    {
        var log = ResolveLogger(logger);
        log.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> INFO");
        if (!string.IsNullOrWhiteSpace(message)) log.LogInformation(message);
        log.LogInformation(e.Message);
        log.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Warning

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogWarning(ILogger? logger, Exception e, string message = "")
    {
        var log = ResolveLogger(logger);
        log.LogWarning(">>>>>>>>>>>>>>>>>>>>>>>>>>>> WARNING");
        if (!string.IsNullOrWhiteSpace(message)) log.LogWarning(message);
        log.LogWarning(e.Message);
        log.LogWarning("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Builds the inner lines shared by both LogError and LogInfo for a Result.
    /// Callers wrap these lines with their own header/footer.
    /// </summary>
    private static IEnumerable<string> FormatResultBlock(Result result, string message)
    {
        if (!string.IsNullOrWhiteSpace(result.TraceCode)) yield return result.TraceCode;
        if (!string.IsNullOrWhiteSpace(message)) yield return message;
        if (!string.IsNullOrWhiteSpace(result.Code)) yield return result.Code;
        if (!string.IsNullOrWhiteSpace(result.Message)) yield return result.Message;

        if (result.Errors.Count > 0)
        {
            yield return "Errors:";
            foreach (var error in result.Errors)
                yield return $"    ⊢ {error.Key}: {string.Join(", ", error.Value)}";
        }
    }
}