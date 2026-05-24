using Backend.Core.Internal;
using Backend.Core.Models;

namespace Backend.Core.Policies;

public class DbRetry
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
    /// <summary>
    ///  Executes an operation with retry policy. in case the operations went wrong, it logs the error.
    /// </summary>
    /// <param name="operation">The operation to execute. It should return a Result object indicating success or failure.</param>
    /// <param name="operationName">The name of the operation for logging purposes.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">The base delay in milliseconds for exponential backoff.</param>
    /// <param name="logger">The logger to use for logging messages.</param>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <returns>A <see cref="Task{Result{T}}"/> instance</returns>
    public static async Task<Result<T>> ExecuteWithRetry<T>(
        Func<Task<Result<T>>> operation,
        string operationName,
        int maxRetries = 3,
        int baseDelayMs = 2000,
        ILogger? logger = null
    )
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await operation();
                if (result || logger is null) return result;

                // Log errors
                logger.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ERROR");
                logger.LogError(
                    "DB operation {OperationName} failed with code {Code} and message: {Message}",
                    operationName,
                    result.Code,
                    result.Message
                );
                logger.LogError("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                return result;
            }
            catch (Exception ex) when (IsTransientException(ex) && attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt));
                logger.LogWarning(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> WARNING");
                logger?.LogWarning(
                    ex,
                    "Transient DB error while {OperationName} (attempt {Attempt}/{Max}). Retrying in {DelayMs}ms...",
                    operationName,
                    attempt + 1,
                    maxRetries + 1,
                    delay.TotalMilliseconds
                );
                logger.LogWarning("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

                await Task.Delay(delay);
            }
        }

        return new Result<T>
        {
            Success = false,
            Code = "DB_ERROR",
            Status = 500,
            Message = $"{operationName} DB operation failed after {maxRetries + 1} attempts.",
            IC = $"{FileCodes.CallerIC()}-{operationName}",
            Returnable = false
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    private static bool IsTransientException(Exception ex)
    {
        return ex switch
        {
            // Npgsql transient errors
            Npgsql.PostgresException pgEx => IsTransientPostgresError(pgEx.SqlState),

            // Network/timeout issues
            TimeoutException => true,
            IOException => true,
            System.Net.Sockets.SocketException => true,

            // EF Core concurrency (if using SaveChanges)
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => true,

            _ => false
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    private static bool IsTransientPostgresError(string sqlState)
    {
        // Common transient PostgreSQL error classes
        return sqlState.StartsWith("08") // Connection exceptions
               || sqlState.StartsWith("53") // Insufficient resources (e.g., too many connections)
               || sqlState.StartsWith("57") // Operator intervention (e.g., shutdown)
               || sqlState == "40001" // Serialization failure (retryable in transactions)
               || sqlState == "40P01"; // Deadlock detected
    }
}