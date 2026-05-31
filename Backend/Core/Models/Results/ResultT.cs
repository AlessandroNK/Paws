using System.Runtime.CompilerServices;
using Backend.Core.Internal;
using Backend.Core.Models.Interfaces;

namespace Backend.Core.Models.Results;

public class Result<T> : Result
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The data of the result. It can be of any type, depending on the operation. For example, it can be a user object,
    /// a list of products, etc.
    /// </summary>
    public T? Data { get; set; }


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------
    public static implicit operator bool(Result<T> result) => result.Success;

    // -----------------------------------------------------------------------------------------------------------------
    public static implicit operator Result<T>(T data)
    {
        return new Result<T>
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Message = "Operation completed successfully",
            Data = data
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static implicit operator Result<T>(bool result)
    {
        return new Result<T>
        {
            Success = result,
            Code = result ? "SUCCESS" : "FAILURE",
            Status = result ? 200 : 400,
            Message = result ? "Operation completed successfully" : "Operation failed",
            Data = default(T),
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
    /// <param name="data">The data associated with the result.</param>
    /// <param name="file">The file where the result was created.</param>
    /// <param name="line">The line number where the result was created.</param>
    /// <returns>A <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Create(
        bool success,
        string code,
        int status,
        bool returnable = true,
        string? message = null,
        T? data = default,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        return new Result<T>
        {
            Success = success,
            Code = code,
            Status = status,
            Message = message,
            TraceCode = $"{Path.GetFileName(file)}:{line}",
            Returnable = returnable,
            Data = data,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns a default success result with no important data
    /// </summary>
    /// <returns></returns>
    public new static Result<T> GetDefaultSuccess()
    {
        return new Result<T>
        {
            Success = true,
            Code = "DEFAULT_SUCCESS_RESULT",
            Status = 500,
            Message = "This is a default result with no data",
            Data = default(T),
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Convert this result to a result of a different type.
    /// </summary>
    /// <typeparam name="TU"></typeparam>
    /// <returns>A result of type TU.</returns>
    /// <remarks>In case the data is not compatible, the new result will drop the data.</remarks>
    public new Result<TU> ConvertTo<TU>()
    {
        return Data is TU data
            ? new Result<TU>
            {
                Success = Success,
                Code = Code,
                Status = Status,
                Message = Message,
                Data = data,
                Errors = Errors,
                TraceCode = TraceCode,
                Returnable = Returnable,
            }
            : new Result<TU>
            {
                Success = Success,
                Code = Code,
                Status = Status,
                Message = Message,
                Data = default(TU),
                Errors = Errors,
                TraceCode = TraceCode,
                Returnable = Returnable,
            };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Cleans and hides internal information when the result is not retornable to keep intern info secure
    /// </summary>
    /// <returns></returns>
    public ResultDtoT<TU> ToDto<TU>()
    {
        // We will filter info as we need so we can
        // return this result to the frontend securely
        var result = new ResultDtoT<TU>
        {
            Success = Success,
            Code = Returnable ? Code : "INTERNAL_ERROR",
            Status = Status,
            Message = Returnable ? Message : "An error occurred in the API",
            Data = default(TU),
            Errors = Returnable ? Errors : new Dictionary<string, string[]>(),
            TraceCode = TraceCode
        };

        if (Data is not IDtoConvertible<TU> convertibleData) return result;

        // We will try to convert the data to a Dto,
        // if it fails we will return the original data
        // as null and log the error
        try
        {
            result.Data = convertibleData.ToDto();
        }
        catch (Exception e)
        {
            // We will log the error and return the original data as null
            LogHelpers.LogError(e, "Error converting the data to a DTO");
            result.Data = default(TU);
            result.Code = "DATA_CONVERSION_ERROR";
            result.Message = "An error occurred while converting the data";
            result.Status = 500;
        }

        return result;
    }
}