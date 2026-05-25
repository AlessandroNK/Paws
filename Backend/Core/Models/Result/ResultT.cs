using System.Runtime.CompilerServices;

namespace Backend.Core.Models.Result;

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
            IC = $"{Path.GetFileName(file)}:{line}",
            Returnable = returnable,
            Data = data,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Convert this result to a result of a different type.
    /// </summary>
    /// <typeparam name="TU"></typeparam>
    /// <returns>A result of type U.</returns>
    /// <remarks>In case the data is not compatible, the new result will drop the data.</remarks>
    public Result<TU> ConvertTo<TU>()
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
                IC = IC,
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
                IC = IC,
                Returnable = Returnable,
            };
    }
}