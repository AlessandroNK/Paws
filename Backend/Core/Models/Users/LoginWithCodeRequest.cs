using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Users;

public class LoginWithCodeRequest
{
    /// <summary>
    /// The email of the user. It is required and must be a valid email address.
    /// </summary>
    [Required (ErrorMessage = "Email is required")]
    [EmailAddress (ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    /// <summary>
    /// The code sent to the user's email. It is required and must be a string of 6 digits.
    /// </summary>
    [Required (ErrorMessage = "Code is required")]
    [StringLength (6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; }
}