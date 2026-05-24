using Backend.Core.Models;

namespace Backend.Core.Internal;

public static class Helpers
{
    // -----------------------------------------------------------------------------------------------------------------
    public static void LogResult(ILogger logger, Result result, string message = "")
    {
        if (result) LogInfo(logger, result, message);
        else LogError(logger, result, message);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogResult<T>(ILogger logger, Result<T> result, string message = "")
    {
        if (result) LogInfo(logger, result, message);
        else LogError(logger, result, message);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Exception e, string message = "")
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrEmpty(message)) logger.LogError(message);
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

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Result result, string message = "")
    {
        logger.LogError("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrWhiteSpace(result.IC)) logger.LogError(result.IC);
        if (!string.IsNullOrWhiteSpace(message)) logger.LogError(message);
        if (!string.IsNullOrWhiteSpace(result.Code)) logger.LogError(result.Code);
        if (!string.IsNullOrWhiteSpace(result.Message)) logger.LogError(result.Message);
        if (!string.IsNullOrEmpty(message)) logger.LogError(message);
        if (result.Errors.Count > 0)
        {
            logger.LogError("Errors:");
            foreach (var error in result.Errors)
                logger.LogError($"    ⊢ {error.Key}: {error.Value}");
        }
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError<T>(ILogger logger, Result<T> result, string message = "")
    {
        logger.LogError("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrWhiteSpace(result.IC)) logger.LogError(result.IC);
        if (!string.IsNullOrWhiteSpace(message)) logger.LogError(message);
        if (!string.IsNullOrWhiteSpace(result.Code)) logger.LogError(result.Code);
        if (!string.IsNullOrWhiteSpace(result.Message)) logger.LogError(result.Message);
        if (!string.IsNullOrEmpty(message)) logger.LogError(message);
        if (result.Errors.Count > 0)
        {
            logger.LogError("Errors:");
            foreach (var error in result.Errors)
                logger.LogError($"    ⊢ {error.Key}: {error.Value}");
        }
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Exception e)
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        logger.LogError(e.Message);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger logger, string message)
    {
        logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> INFO");
        logger.LogInformation(message);
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo(ILogger logger, Result result, string message = "")
    {
        logger.LogInformation("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> SUCCESS");
        if (!string.IsNullOrWhiteSpace(result.IC)) logger.LogInformation(result.IC);
        if (!string.IsNullOrWhiteSpace(message)) logger.LogInformation(message);
        if (!string.IsNullOrWhiteSpace(result.Code)) logger.LogInformation(result.Code);
        if (!string.IsNullOrWhiteSpace(result.Message)) logger.LogInformation(result.Message);
        if (!string.IsNullOrEmpty(message)) logger.LogInformation(message);
        if (result.Errors.Count > 0)
        {
            logger.LogInformation("Errors:");
            foreach (var error in result.Errors)
                logger.LogInformation($"    ⊢ {error.Key}: {error.Value}");
        }
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static void LogInfo<T>(ILogger logger, Result<T> result, string message = "")
    {
        logger.LogInformation("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> SUCCESS");
        if (!string.IsNullOrWhiteSpace(result.IC)) logger.LogInformation(result.IC);
        if (!string.IsNullOrWhiteSpace(message)) logger.LogInformation(message);
        if (!string.IsNullOrWhiteSpace(result.Code)) logger.LogInformation(result.Code);
        if (!string.IsNullOrWhiteSpace(result.Message)) logger.LogInformation(result.Message);
        if (!string.IsNullOrEmpty(message)) logger.LogInformation(message);
        if (result.Errors.Count > 0)
        {
            logger.LogInformation("Errors:");
            foreach (var error in result.Errors)
                logger.LogInformation($"    ⊢ {error.Key}: {error.Value}");
        }
        logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
    }
}