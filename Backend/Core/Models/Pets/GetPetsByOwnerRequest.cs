using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Pets;

public class GetPetsByOwnerRequest
{
    /// <summary>
    /// A unique identifier for the ownership.
    /// </summary>
    [Required(ErrorMessage = "Owner ID is required.")]
    public int OwnerId { get; set; }
}