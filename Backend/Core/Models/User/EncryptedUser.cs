using Backend.Core.Models.Enums;

namespace Backend.Core.Models.User;

/// <summary>
///  This is a user but all data is encrypted so we can store it in the database without worrying about security.
/// </summary>
public class EncryptedUser
{
    /// <summary>
    /// The id of the user in the database
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// A hash of the user's password. It is used to verify the user's password when signing in.
    /// </summary>
    public string PasswordHashed { get; set; } = string.Empty;

    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    public string EmailEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// A hash of the email to find it quickly.
    /// </summary>
    public string EmailHash { get; set; }  = string.Empty;

    /// <summary>
    /// The tpe of the document of the user.
    /// </summary>
    public DocumentType DocumentType { get; init; }

    /// <summary>
    /// The number of identification like we are simple elements named by random numbers
    /// </summary>
    public string DocumentNumberEncrypted { get; set; } = string.Empty;

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
    public UserStatus Status { get; set; }

    /// <summary>
    /// Any verification code for the user.
    /// </summary>
    public string VerificationCodeEncrypted { get; set; } = string.Empty;
}