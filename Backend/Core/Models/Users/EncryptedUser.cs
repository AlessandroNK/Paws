using Backend.Core.Models.Enums;
using Backend.Core.Models.Relationships;

namespace Backend.Core.Models.Users;

/// <summary>
///  This is a user but all data is encrypted so we can store it in the database without worrying about security.
/// </summary>
public class EncryptedUser
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
/// <summary>
    /// The id of the user in the database
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// A hash of the user's password. It is used to verify the user's password when signing in.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    public string EncryptedEmail { get; set; } = string.Empty;

    /// <summary>
    /// A hash of the email to find it quickly.
    /// </summary>
    public string EmailHash { get; set; }  = string.Empty;

    /// <summary>
    /// The tpe of the document of the user.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// The number of identification like we are simple elements named by random numbers
    /// </summary>
    public string EncryptedDocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// A hash to quickly find the user by the document number
    /// </summary>
    public string DocumentHash { get; set; }

    /// <summary>
    /// The name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the user was created. It is used to track when the user was created and to sort users by
    /// creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the user was last updated. It is used to track when the user was last updated and to sort
    /// users
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// A flag to track user status in the system.
    /// </summary>
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Any verification code for the user.
    /// </summary>
    public string EncryptedVerificationCode { get; set; } = string.Empty;

    /// <summary>
    /// A timestamp indication the time when the code was created. This is useful to invalidate the code after a certain
    /// time has passed and to prevent sending codes to the user until the previous code has expired.
    /// </summary>
    public DateTime VerificationCodeDate { get; set; }

    /// <summary>
    /// All user's pets
    /// </summary>
    public List<EncryptedUserPet> UserPets { get; set; } = new List<EncryptedUserPet>();


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