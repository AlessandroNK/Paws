using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Users;

/// <summary>
/// Represents a sign up request. It contains the user's email and password. It is used to sign in an existing user.
/// It is used in the sign in endpoint of the user controller.
/// </summary>
public class SignInRequest
{
    /// <summary>
    /// User's email
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public required string Email { get; set; }

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public required string Password { get; set; }
}