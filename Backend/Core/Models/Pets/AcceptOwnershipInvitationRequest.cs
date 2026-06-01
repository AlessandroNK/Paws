using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Pets;

public class AcceptOwnershipInvitationRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    [Required(ErrorMessage = "New owner email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string NewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// A unique identifier for the ownership invitation. It is used to verify the invitation when the new owner clicks
    /// the link in the email.
    /// </summary>
    [Required(ErrorMessage = "Ownership invitation code is required.")]
    public string InvitationCode { get; set; }  = string.Empty;


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