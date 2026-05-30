using Backend.Core.Models.Enums;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Users;

namespace Backend.Core.Models.Relationships;

public class EncryptedUserPet
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the user-pet relationship in the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The id of the user in the relationship.
    /// </summary>
    public int EncryptedUserId { get; set; }

    /// <summary>
    /// The user of the relationship
    /// </summary>
    public EncryptedUser EncryptedUser { get; set; }

    /// <summary>
    /// The id of the pet in the relationship.
    /// </summary>
    public int EncryptedPetId { get; set; }

    /// <summary>
    /// The pet of the relationship
    /// </summary>
    public EncryptedPet EncryptedPet { get; set; }

    /// <summary>
    /// The date and time when the relationship was created. It is used to track when the relationship was created and to sort relationships by
    /// creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the relationship was last updated. It is used to track when the relationship was last updated and to sort
    /// relationships
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// A flag to track relationship status in the system.
    /// </summary>
    public GenericStatus Status { get; set; }


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

}