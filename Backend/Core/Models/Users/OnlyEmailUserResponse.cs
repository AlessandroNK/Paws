namespace Backend.Core.Models.Users;

public struct OnlyEmailUserResponse
{
    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    public string Email { get; set; }
}