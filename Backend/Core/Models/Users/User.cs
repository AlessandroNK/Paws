using System.ComponentModel.DataAnnotations.Schema;
using Backend.Core.Internal;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Services;

namespace Backend.Core.Models.Users;

/// <summary>
/// Represents a user in the API
/// </summary>
public class User : IDtoConvertible<UserResponse>,
    IDtoConvertible<BasicUserResponse>,
    IDtoConvertible<OnlyEmailUserResponse>,
    IEncryptable
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
    public DateTime? VerificationCodeDate { get; set; }

    /// <summary>
    /// All user's pets
    /// </summary>
    public List<UserPet> UserPets { get; set; } = new List<UserPet>();

    /// <summary>
    /// User's session token, this is used to authenticate the user in the frontend and to authorize the user to access
    /// certain endpoints in the backend.
    /// </summary>
    public SessionToken SessionToken { get; set; }

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
    OnlyEmailUserResponse IDtoConvertible<OnlyEmailUserResponse>.ToDto()
    {
        return new OnlyEmailUserResponse
        {
            Email = Email,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public Result Hash()
    {
        // ------------------------------------------------------------------------------ Password
        if (string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(PasswordHash))
            return new Result
            {
                Success = false,
                Code = "PASSWORD_NOT_PROVIDED",
                Status = 400,
                Title = "Password not provided for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };

        // If no new password, then use the existing hash
        var passwordResult = !string.IsNullOrWhiteSpace(Password)
            ? SecurityService.HashWithSalt(Password)
            : PasswordHash;

        if (!passwordResult || passwordResult.Data is null)
            return new Result
            {
                Success = false,
                Code = "PASSWORD_HASHING_FAILED",
                Status = 500,
                Title = "Password hashing failed for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };
        PasswordHash = passwordResult.Data;

        // ------------------------------------------------------------------------------ Document number
        if (string.IsNullOrWhiteSpace(DocumentNumber))
            return new Result
            {
                Success = false,
                Code = "DOCUMENT_NOT_PROVIDED",
                Status = 400,
                Title = "Document not provided for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };

        var documentHashResult = SecurityService.HashWithSalt(DocumentNumber);
        if (!documentHashResult || documentHashResult.Data is null)
            return new Result
            {
                Success = false,
                Code = "DOCUMENT_HASHING_FAILED",
                Status = 500,
                Title = "Document hashing failed for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };
        DocumentHash = documentHashResult.Data;

        // ------------------------------------------------------------------------------ Email
        if (string.IsNullOrWhiteSpace(Email))
            return new Result
            {
                Success = false,
                Code = "EMAIL_NOT_PROVIDED",
                Status = 400,
                Title = "Email not provided for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };

        var emailResult = SecurityService.HashWithSalt(Email);
        if (!emailResult || emailResult.Data is null)
            return new Result
            {
                Success = false,
                Code = "EMAIL_HASHING_FAILED",
                Status = 500,
                Title = "Email hashing failed for user hash",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };
        EmailHash = emailResult.Data;

        return new Result
        {
            Success = true,
            Code = "USER_HASHED",
            Status = 200,
            Title = "User hashed successfully",
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = false
        };
    }
}