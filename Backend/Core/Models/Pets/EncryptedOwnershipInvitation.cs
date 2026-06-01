using Backend.Core.Models.Enums;
using Backend.Core.Models.Users;

namespace Backend.Core.Models.Pets;

public class EncryptedOwnershipInvitation
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The ID of the ownership invitation
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The ID of the user sharing ownership of the pet.
    /// </summary>
    public int EncryptedUserId { get; set; }

    /// <summary>
    /// The user sharing ownership of the pet. It is included in the invitation to avoid an extra database query when the
    /// new owner clicks the link in the email.
    /// </summary>
    public EncryptedUser EncryptedUser { get; set; }

    /// <summary>
    /// The ID of the pet to share ownership of.
    /// </summary>
    public int EncryptedPetId { get; set; }

    /// <summary>
    /// The pet that is being shared
    /// </summary>
    public EncryptedPet EncryptedPet { get; set; }

    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    public string EncryptedNewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user to share ownership with.
    /// </summary>
    public string EncryptedNewOwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the email of the new owner is already registered in the database. If true, we can directly add the pet
    /// to the new owner's account once they clicks the link.
    /// </summary>
    public bool NewOwnerHasAccount { get; set; }

    /// <summary>
    /// The timestamp when this ownership invitation will expire.
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// A unique identifier for the ownership invitation. It is used to verify the invitation when the new owner clicks
    /// the link in the email.
    /// </summary>
    public string EncryptedInvitationCode { get; set; }  = string.Empty;

    /// <summary>
    /// A hash of the invitation codeso it is easy to find in the database.
    /// </summary>
    public string InvitationCodeHash { get; set; }

    /// <summary>
    /// Ya know
    /// </summary>
    public EntityStatus Status { get; set; }


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
