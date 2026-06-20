using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Users;

public class LoginRequest
{
    /// <summary>
    /// The email of the user. It is required and must be a valid email address.
    /// </summary>
    [Required (ErrorMessage = "Email is required")]
    [EmailAddress (ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
}