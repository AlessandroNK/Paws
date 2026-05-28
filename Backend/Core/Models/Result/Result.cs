using System.Runtime.CompilerServices;
using Backend.Core.Internal;

namespace Backend.Core.Models.Result;

public class Result
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Whether the result is ok or not. It is used to indicate whether the operation was successful or not.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Alias for <see cref="Success"/>
    /// </summary>
    public bool Ok => Success;

    /// <summary>
    /// Alias for <see cref="Success"/>
    /// </summary>
    public bool IsSuccess => Success;

    /// <summary>
    /// A custom code in SCREAMING_SNAKE_CASE
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The int number that represents the current status of the system.
    /// </summary>
    public required int Status { get; set; }

    /// <summary>
    /// A custom message to show to the user
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Any validation errors associated with the result.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; }  = new Dictionary<string, string[]>();

    /// <summary>
    /// An internal code to identify the result origin easily
    /// </summary>
    public string TraceCode { get; set; } = string.Empty;

    /// <summary>
    /// Whether the result should be returned to the frontend or not. In case it does not, it should be replaced for a
    /// generic response before returning it to the frontend
    /// </summary>
    public bool Returnable { get; set; }


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------
    public static implicit operator bool(Result result) => result.Success;

    // -----------------------------------------------------------------------------------------------------------------
    public static implicit operator Result(bool result)
    {
        return new Result
        {
            Success = result,
            Code = result ? "SUCCESS" : "FAILURE",
            Status = result ? 200 : 400,
            Message = result ? "Operation completed successfully" : "Operation failed",
            Returnable = true
        };
    }


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a result
    /// </summary>
    /// <param name="success">Whether the result was success or not</param>
    /// <param name="code">A code to identify the result. It can be used to identify the type of error or success.</param>
    /// <param name="status">The HTTP status code for the result.</param>
    /// <param name="returnable">Whether the result should be returned to the client.</param>
    /// <param name="message">A message describing the result.</param>
    /// <param name="file">The file where the result was created.</param>
    /// <param name="line">The line number where the result was created.</param>
    /// <returns>A <see cref="Result"/> instance.</returns>
    public static Result Create(
        bool success,
        string code,
        int status,
        bool returnable = true,
        string? message = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        return new Result
        {
            Success = success,
            Code = code,
            Status = status,
            Message = message,
            TraceCode = $"{Path.GetFileName(file)}:{line}",
            Returnable = returnable,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public Result AddError(string error, string[] messages)
    {
        Errors.Add(error, messages);
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Convert this result to a result of a different type.
    /// </summary>
    /// <typeparam name="TU"></typeparam>
    /// <returns>A result of type U.</returns>
    /// <remarks>The new result will drop the data.</remarks>
    public Result<TU> ConvertTo<TU>()
    {
        return new Result<TU>
        {
            Success = Success,
            Code = Code,
            Status = Status,
            Message = Message,
            Errors = Errors,
            TraceCode = TraceCode,
            Returnable = Returnable,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Logs the result using the provided logger and returns the same result for chaining.
    /// </summary>
    /// <param name="logger">The logger to use for logging the result.</param>
    /// <param name="message">An optional message to include in the log.</param>
    public Result Log(ILogger logger, string message = "")
    {
        LogHelpers.LogResult(logger, this, message);
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Cleans and hides internal information when the result is not retorneable to keep intern info secure
    /// </summary>
    /// <returns></returns>
    public ResultDto CleanToReturn()
    {
        // We will filter info as we need so we can
        // return this result to the frontend securely
        return new ResultDto
        {
            Success = Success,
            Code = Returnable ? Code : "INTERNAL_ERROR",
            Status = Status,
            Message = Returnable ? Message : "An error occurred in the API",
            Errors = Returnable ? Errors : new Dictionary<string, string[]>(),
            TraceCode = TraceCode
        };
    }
}