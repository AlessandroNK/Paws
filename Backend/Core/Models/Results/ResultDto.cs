namespace Backend.Core.Models.Results;

public class ResultDto
{
    //                                                                                                Private Properties
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
}