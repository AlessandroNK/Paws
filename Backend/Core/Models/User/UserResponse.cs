using Backend.Core.Models.Enums;

namespace Backend.Core.Models.User;

/// <summary>
/// A DTO so we can return the user data to the frontend without returning the password hash or any other unwanted data.
/// </summary>
public struct UserResponse
{
    /// <summary>
    /// The id of the user in the database
    /// </summary>
    public int Id  { get; init; }

    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The tpe of the document of the user.
    /// </summary>
    public DocumentType DocumentType { get; init; }

    /// <summary>
    /// The number of identification like we are simple elements named by random numbers
    /// </summary>
    public string DocumentNumber { get; set; }

    /// <summary>
    /// The name of the user.
    /// </summary>
    public string Name { get; set; }
}