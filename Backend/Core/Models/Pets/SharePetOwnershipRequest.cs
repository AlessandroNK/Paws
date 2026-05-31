using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Pets;

public class SharePetOwnershipRequest
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
    /// The email of the user to share ownership with.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; set; } = string.Empty;
}