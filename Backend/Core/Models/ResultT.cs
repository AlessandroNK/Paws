namespace Backend.Core.Models;

public class Result<T>
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Whether the result is ok or not. It is used to indicate whether the operation was successful or not.
    /// </summary>
    public bool Ok { get; set; }

    /// <summary>
    /// Alias for Ok
    /// </summary>
    public bool IsSuccess => Ok;

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
    /// The data of the result. It can be of any type, depending on the operation. For example, it can be a user object,
    /// a list of products, etc.
    /// </summary>
    public T Data { get; set; }


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
}