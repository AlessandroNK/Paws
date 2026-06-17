using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Results;

public class ApiResponse
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
    /// The kind of the response. It is used to indicate the type of the response, such as success, error, validation
    /// error, etc.
    /// </summary>
    public ApiResponseKind Kind { get; set; }

    /// <summary>
    /// A custom code in SCREAMING_SNAKE_CASE
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The int number that represents the current status of the system.
    /// </summary>
    public required int Status { get; set; }

    /// <summary>
    /// A custom title to show to the user
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Any validation errors associated with the result.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; }  = new Dictionary<string, string[]>();

    /// <summary>
    /// An internal code to identify the result origin easily
    /// </summary>
    public string TraceCode { get; set; } = string.Empty;
}