using Backend.Core.Models;
using Backend.Core.Models.Result;

namespace Backend.Core.Internal;

public static class LogHelpers
{
    // -----------------------------------------------------------------------------------------------------------------
    public static void LogResult(ILogger logger, Result result, string message = "")
    {
        // Result<T> inherits Result, so this single overload handles both Result and Result<T>.
        if (result) LogInfo(logger, result, message);
        else LogError(logger, result, message);
    }

    #region Error

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Result result, string message = "")
    {
        logger.LogError("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        foreach (var line in FormatResultBlock(result, message))
            logger.LogError(line);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Exception e, string message = "")
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrWhiteSpace(message)) logger.LogError(message);
        logger.LogError(e.Message);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, string message)
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        logger.LogError(message);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Info

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger logger, Result result, string message = "")
    {
        logger.LogInformation("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> SUCCESS");
        foreach (var line in FormatResultBlock(result, message))
            logger.LogInformation(line);
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger logger, string message)
    {
        logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> INFO");
        logger.LogInformation(message);
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger logger, Exception e, string message = "")
    {
        logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> INFO");
        if (!string.IsNullOrWhiteSpace(message)) logger.LogInformation(message);
        logger.LogInformation(e.Message);
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Warning

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogWarning(ILogger logger, Exception e, string message = "")
    {
        logger.LogWarning(">>>>>>>>>>>>>>>>>>>>>>>>>>>> WARNING");
        if (!string.IsNullOrWhiteSpace(message)) logger.LogWarning(message);
        logger.LogWarning(e.Message);
        logger.LogWarning("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Builds the inner lines shared by both LogError and LogInfo for a Result.
    /// Callers wrap these lines with their own header/footer.
    /// </summary>
    private static IEnumerable<string> FormatResultBlock(Result result, string message)
    {
        if (!string.IsNullOrWhiteSpace(result.IC)) yield return result.IC;
        if (!string.IsNullOrWhiteSpace(message)) yield return message;
        if (!string.IsNullOrWhiteSpace(result.Code)) yield return result.Code;
        if (!string.IsNullOrWhiteSpace(result.Message)) yield return result.Message;

        if (result.Errors?.Count > 0)
        {
            yield return "Errors:";
            foreach (var error in result.Errors)
                yield return $"    ⊢ {error.Key}: {string.Join(", ", error.Value)}";
        }
    }
}