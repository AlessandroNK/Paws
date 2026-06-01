using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Users;

public class AccountVerificationRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The email of the user sending the request
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The verification code
    /// </summary>
    [Required(ErrorMessage = "Verification code is required.")]
    [RegularExpression("^[A-Z0-9]{6}$", ErrorMessage = "Verification code must be exactly 6 characters and contain only uppercase letters and digits.")]
    public string VerificationCode { get; set; } = string.Empty;


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