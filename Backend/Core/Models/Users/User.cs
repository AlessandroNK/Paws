using System.ComponentModel.DataAnnotations.Schema;
using Backend.Core.Internal;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Services;

namespace Backend.Core.Models.Users;

/// <summary>
/// Represents a user in the API
/// </summary>
public class User : IDtoConvertible<UserResponse>, IDtoConvertible<BasicUserResponse>, IEncryptable
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the user in the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A hash of the user's password. It is used to verify the user's password when signing in.
    /// </summary>
    [NotMapped]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// A hash of the user's password. It is used to verify the user's password when signing in.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    [EncryptProperty]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// A hash of the email so it is easy to find
    /// </summary>
    public string EmailHash { get; set; } = string.Empty;

    /// <summary>
    /// The type of the document of the user.
    /// </summary>
    public DocumentType DocumentType { get; init; }

    /// <summary>
    /// The number of identification like we are simple elements named by random numbers
    /// </summary>
    [EncryptProperty]
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// A hash of the document number so it is easy to find
    /// </summary>
    public string DocumentHash { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user.
    /// </summary>
    [EncryptProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the user was created. It is used to track when the user was created and to sort users by
    /// creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

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
    [EncryptProperty]
    public string VerificationCode { get; set; }

    /// <summary>
    /// A timestamp indication the time when the code was created. This is useful to invalidate the code after a certain
    /// time has passed and to prevent sending codes to the user until the previous code has expired.
    /// </summary>
    public DateTime VerificationCodeDate { get; set; }

    /// <summary>
    /// All user's pets
    /// </summary>
    public List<UserPet> UserPets { get; set; } = new List<UserPet>();


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
    UserResponse IDtoConvertible<UserResponse>.ToDto()
    {
        try
        {
            return new UserResponse
            {
                Id = Id,
                Email = Email,
                DocumentType = DocumentType,
                DocumentNumber = DocumentNumber,
                Name = Name,
                Pets = UserPets.Select(userPet =>
                {
                    IDtoConvertible<PetResponse> petResponse = userPet.Pet;
                    return petResponse.ToDto();
                }).ToList()
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(e);
            return new UserResponse
            {
                Id = Id,
                Email = Email,
                DocumentType = DocumentType,
                DocumentNumber = DocumentNumber,
                Name = Name,
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    BasicUserResponse IDtoConvertible<BasicUserResponse>.ToDto()
    {
        return new BasicUserResponse
        {
            Id = Id,
            Email = Email,
            DocumentType = DocumentType,
            DocumentNumber = DocumentNumber,
            Name = Name
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public void Hash()
    {
        PasswordHash = SecurityService.HashString(Password);
        EmailHash = SecurityService.HashString(Email);
        DocumentHash = SecurityService.HashString(DocumentNumber);
    }
}