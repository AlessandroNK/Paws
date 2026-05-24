using System.ComponentModel.DataAnnotations;

namespace Backend.Core.DTOs.Requests;

/// <summary>
/// Represents the request body for signing up a new user. It contains the user's email and password. It is used to create
/// a new user in the system.
/// </summary>
public struct SignUpRequest
{
    /// <summary>
    /// User's email
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; set; }

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    /// <summary>
    /// User's document type
    /// </summary>
    [Range(1, 3, ErrorMessage = "Document type must be between 1 and 3.")]
    public int DocumentType { get; set; }

    /// <summary>
    /// User's document number
    /// </summary>
    [Required(ErrorMessage = "Document number is required.")]
    [StringLength(15, MinimumLength = 1, ErrorMessage = "Document number must be between 1 and 15 characters.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Document number can only contain digits (0-9).")]
    public string DocumentNumber { get; set; }

    /// <summary>
    /// User's Name
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "The name cannot exceed 200 characters.")]
    [MinLength(1, ErrorMessage = "The name must be at least 1 character long.")]
    [RegularExpression(@"^[\p{L}\p{M}\s'\-]+$",
        ErrorMessage = "The name can only contain letters, spaces, hyphens, and apostrophes.")]
    public string Name { get; set; }
}