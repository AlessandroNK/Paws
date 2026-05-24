namespace Backend.Core.Internal;

public static class Helpers
{
    // -----------------------------------------------------------------------------------------------------------------
    public static void LogError(ILogger logger, Exception e)
    {
        logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
        logger.LogError(e.Message);
        logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
    }
}