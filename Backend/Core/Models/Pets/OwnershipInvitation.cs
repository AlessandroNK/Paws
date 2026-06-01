using Backend.Core.Models.Enums;
using Backend.Core.Models.Users;

namespace Backend.Core.Models.Pets;

public class OwnershipInvitation
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
    public int UserId { get; set; }

    /// <summary>
    /// The owner object
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// The ID of the pet to share ownership of.
    /// </summary>
    public int PetId { get; set; }

    /// <summary>
    /// The pet of the invitation
    /// </summary>
    public Pet? Pet { get; set; }

    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    public string NewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user to share ownership with.
    /// </summary>
    public string NewOwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Whenter the email of the new owner is already registered in the database. If true, we can directly add the pet
    /// to the new owner's acoount once they ckicks the link.
    /// </summary>
    public bool NewOwnerHasAccount { get; set; }

    /// <summary>
    /// The timestamp when this ownership invitation will expire
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// A unique identifier for the ownership invitation. It is used to verify the invitation when the new owner clicks
    /// the link in the email.
    /// </summary>
    public string InvitationCode { get; set; }  = string.Empty;

    /// <summary>
    /// A unique link that the new owner can click to accept the invitation.
    /// </summary>
    public string OwnershipLink { get; set; }

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