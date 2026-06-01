using Backend.Core.Models.Enums;
using Backend.Core.Models.Users;

namespace Backend.Core.Models.Pets;

public class EncryptedShareInvitation
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The ID of the shared invitation
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The ID of the user sharing ownership of the pet.
    /// </summary>
    public int EncryptedUserId { get; set; }

    /// <summary>
    /// The ID of the pet to share ownership of.
    /// </summary>
    public int EncryptedPetId { get; set; }

    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    public string EncryptedNewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user to share ownership with.
    /// </summary>
    public string EncryptedNewOwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Whenter the email of the new owner is already registered in the database. If true, we can directly add the pet
    /// to the new owner's acoount once they ckicks the link.
    /// </summary>
    public bool NewOwnerHasAccount { get; set; }

    /// <summary>
    /// The timestamp when this shared code will expire
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// A unique identifier for the share invitation. It is used to verify the invitation when the new owner clicks the
    /// link in the email.
    /// </summary>
    public string EncryptedNonce { get; set; }  = string.Empty;

    /// <summary>
    /// A hash of the nonce so it is easy ro find in the db
    /// </summary>
    public string NonceHash { get; set; }

    /// <summary>
    /// Ya know
    /// </summary>
    public GenericStatus Status { get; set; }


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