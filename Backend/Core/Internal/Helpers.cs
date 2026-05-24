namespace Backend.Core.Internal;

public static class Helpers
{
    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Exception e, string message = "")
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        if (!string.IsNullOrEmpty(message)) logger.LogError(message);
        logger.LogError(e.Message);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }
}