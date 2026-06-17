using System.ComponentModel.DataAnnotations.Schema;
using Backend.Core.Internal;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Services;

namespace Backend.Core.Models.Pets;

public class OwnershipInvitation : IEncryptable
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
    [EncryptProperty]
    public string NewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user to share ownership with.
    /// </summary>
    [EncryptProperty]
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
    [EncryptProperty]
    public string InvitationCode { get; set; } = string.Empty;

    /// <summary>
    /// A hash for the unique identifier for the ownership invitation. It is used to verify the invitation when the new
    /// owner clicks
    /// the link in the email.
    /// </summary>
    public string InvitationCodeHash { get; set; } = string.Empty;

    /// <summary>
    /// A unique link that the new owner can click to accept the invitation.
    /// </summary>
    [NotMapped]
    public string OwnershipLink { get; set; } = string.Empty;

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
    public Result Hash()
    {
        if (string.IsNullOrWhiteSpace(InvitationCode))
            return new Result
            {
                Success = false,
                Code = "INVALID_INVITATION_CODE",
                Status = 400,
                Title = "Invitation code cannot be null or empty.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };

        var invitationResult = SecurityService.HashWithSalt(InvitationCode);
        if (!invitationResult || invitationResult.Data is null)
            return new Result
            {
                Success = false,
                Code = "HASHING_ERROR",
                Status = 500,
                Title = "An error occurred while hashing the invitation code.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };

        InvitationCodeHash = invitationResult.Data;
        return new Result
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Title = "Invitation code hashed successfully.",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }
}