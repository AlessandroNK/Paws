using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Relationships;

namespace Backend.Core.Models.Pets;

public class Pet : IDtoConvertible<PetResponse>, IEncryptable
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the pet in the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the pet.
    /// </summary>
    [EncryptProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The hash of the pet's name.
    /// </summary>
    public string NameHash { get; set; } = string.Empty;

    /// <summary>
    /// The species of the pet
    /// </summary>
    public PetSpecies Species { get; set; } = PetSpecies.Other;

    /// <summary>
    /// The breed of the pet. it will depend on
    /// </summary>
    [EncryptProperty]
    public string Breed { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the pet was created. It is used to track when the pet was created and to sort pets by
    /// creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the pet was last updated. It is used to track when the pet was last updated and to sort
    /// pets
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// A flag to track pet status in the system.
    /// </summary>
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Temporary invitations to share ownership of this pet.
    /// </summary>
    public List<OwnershipInvitation> OwnershipInvitations { get; set; } = new List<OwnershipInvitation>();

    /// <summary>
    /// All pet's owners
    /// </summary>
    public List<UserPet> UserPets { get; set; } = new List<UserPet>();


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    PetResponse IDtoConvertible<PetResponse>.ToDto()
    {
        return new PetResponse
        {
            Id = Id,
            Name = Name,
            Species = Species,
            Breed = Breed,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
        };
    }
    // -----------------------------------------------------------------------------------------------------------------
    public void Hash()
    {
        NameHash = Name.GetHashCode().ToString();
    }
}