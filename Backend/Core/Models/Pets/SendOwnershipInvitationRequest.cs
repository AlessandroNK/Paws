using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Pets;

public class SendOwnershipInvitationRequest
{
    /// <summary>
    /// The ID of the user sharing ownership of the pet.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public int UserId { get; set; }

    /// <summary>
    /// The ID of the pet to share ownership of.
    /// </summary>
    [Required(ErrorMessage = "Pet ID is required.")]
    public int PetId { get; set; }

    /// <summary>
    /// The name of the new owner, this will be used in the invitation email and in the notification to the new owner.
    /// </summary>
    [Required(ErrorMessage = "New owner name is required.")]
    [MinLength(2, ErrorMessage = "New owner name must be at least 2 characters long.")]
    [MaxLength(100, ErrorMessage = "New owner name cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']*$", ErrorMessage = "New owner name can only contain letters, spaces, hyphens, and apostrophes.")]
    public string NewOwnerName { get; set; } = string.Empty;

    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string NewOwnerEmail { get; set; } = string.Empty;
}