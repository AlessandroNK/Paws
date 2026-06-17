using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Results;

public class ApiResponseT<T> : ApiResponse
{
    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The data of the result. It can be of any type, depending on the operation. For example, it can be a user object,
    /// a list of products, etc.
    /// </summary>
    public T? Data { get; set; }
}