using Backend.Core.Data;
using Backend.Core.DTOs.SensitiveData;
using Backend.Core.Internal;
using Backend.Core.Models;
using Backend.Core.Models.Enums;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

/// <summary>
/// provides functionality to access and manage users in the application.
/// This interface is responsible for managing users in the application.
/// It provides methods for creating, updating, deleting, and retrieving users.
/// The implementation of this interface should handle the data access logic, such as interacting with a database
/// or any other data source.
/// </summary>
public class UserRepository(
    ApplicationDbContext dbContext,
    ILogger<UserRepository> logger
) : IUserRepository
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The database context used to interact with the database.
    /// This context is used to perform CRUD operations on the <see cref="User"/> and
    /// <see cref="DocumentType"/> entities.
    /// It is injected into the repository to provide access to the underlying database.
    /// </summary>
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// The logger used to log messages.
    /// </summary>
    private readonly ILogger<UserRepository> _log = logger;


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


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

    #region DB

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email of the user</param>
    /// <param name="excludeHidden">Whether to filter out hidden users</param>
    /// <returns>The user if any</returns>
    public async Task<Result<User?>> GetByEmailAsync(string email, bool excludeHidden)
    {
        // Encrypt user data to find it in the db
        var hashedEmailResult = SecurityService.HashWithSalt(email);
        if (!hashedEmailResult || hashedEmailResult.Data == null)
        {
            _log.LogError("Failed to encrypt email for user lookup: {ErrorMessage}", hashedEmailResult.Message);
            return hashedEmailResult.ConvertTo<User?>();
        }

        // Find the user
        var query = _dbContext.EncryptedUsers.AsQueryable();

        query = excludeHidden
            ? query.Where(u => u.EmailHash == hashedEmailResult.Data && u.Status != UserStatus.Inactive)
            : query.Where(u => u.EmailHash == hashedEmailResult.Data);

        query = query
            .Include(u => u.DocumentType)
            // .Include(u => u.Tokens)
            .AsSplitQuery();

        // Execute query
        var encryptedUser = await query.FirstOrDefaultAsync();
        if (encryptedUser is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Message = "No user found with the provided email",
                IC = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return user
        return DecryptUser(encryptedUser);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="excludeHidden">Whether to filter out hidden users</param>
    /// <returns>The user if any</returns>
    public async Task<Result<User?>> GetByDocumentAsync(string document, bool excludeHidden)
    {
        // Encrypt user data to find it in the db
        var hashedDocumentResult = SecurityService.HashWithSalt(document);
        if (!hashedDocumentResult || hashedDocumentResult.Data == null)
        {
            _log.LogError("Failed to encrypt document for user lookup: {ErrorMessage}", hashedDocumentResult.Message);
            return hashedDocumentResult.ConvertTo<User?>();
        }

        // Find the user
        var query = _dbContext.EncryptedUsers.AsQueryable();

        query = excludeHidden
            ? query.Where(u => u.DocumentHash == hashedDocumentResult.Data && u.Status != UserStatus.Inactive)
            : query.Where(u => u.DocumentHash == hashedDocumentResult.Data);

        query = query
            .Include(u => u.DocumentType)
            // .Include(u => u.Tokens)
            .AsSplitQuery();

        // Execute query
        var encryptedUser = await query.FirstOrDefaultAsync();
        if (encryptedUser is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Message = "No user found with the provided document",
                IC = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return user
        return DecryptUser(encryptedUser);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the sign up was successful</returns>
    public async Task<Result> AddAsync(User user)
    {
        // Encrypt user data
        var result = EncryptUser(user);
        if (!result || result.Data is null) return result;

        // Save the user
        _dbContext.EncryptedUsers.Add(result.Data);
        var saved = await _dbContext.SaveChangesAsync();
        return saved > 0;
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Helpers

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Encrypts sensitive data so we can be sure no one is gonna steal it (no one are going to though)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private Result<EncryptedUser> EncryptUser(User user)
    {
        try
        {
            // Encrypt elements
            //---------------------------------------------------------------------- Password
            var passwordResult = SecurityService.HashWithSalt(user.Password);
            if (!passwordResult || passwordResult.Data == null)
            {
                _log.LogError("Failed to hash user password: {ErrorMessage}", passwordResult.Message);
                return passwordResult.ConvertTo<EncryptedUser>();
            }

            //------------------------------------------------------------------------- Email
            var emailResult = SecurityService.EncryptString(user.Email);
            if (!emailResult || emailResult.Data == null)
            {
                _log.LogError("Failed to encrypt user email: {ErrorMessage}", emailResult.Message);
                return emailResult.ConvertTo<EncryptedUser>();
            }

            //------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.EncryptString(user.DocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
            {
                _log.LogError("Failed to encrypt user document number: {ErrorMessage}", documentNumberResult.Message);
                return documentNumberResult.ConvertTo<EncryptedUser>();
            }

            //------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.EncryptString(user.VerificationCode);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
            {
                _log.LogError("Failed to encrypt user verification code: {ErrorMessage}",
                    verificationCodeResult.Message);
                return verificationCodeResult.ConvertTo<EncryptedUser>();
            }

            return new EncryptedUser
            {
                Id = user.Id,
                EncryptedEmail = emailResult.Data,
                HashedPassword = passwordResult.Data,
                DocumentType = user.DocumentType,
                EncryptedDocumentNumber = documentNumberResult.Data,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                EncryptedVerificationCode = verificationCodeResult.Data,
            };
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to encrypt user data");
            return new Result<EncryptedUser>
            {
                Success = false,
                Code = "USER_ENCRYPTION_FAILED",
                Status = 500,
                Message = "Failed to encrypt user data",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Decrypts sensitive data so we can use it and return it to the frontend
    /// </summary>
    /// <param name="encryptedUser"></param>
    /// <returns></returns>
    private Result<User?> DecryptUser(EncryptedUser? encryptedUser)
    {
        try
        {
            if (encryptedUser is null) return new Result<User?>
            {
                Success = false,
                Code = "ENCRYPTED_USER_NULL",
                Status = 500,
                Message = "no encrypted user to decrypt",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

            // Decrypt elements
            //------------------------------------------------------------------------- Email
            var emailResult = SecurityService.DecryptString(encryptedUser.EncryptedEmail);
            if (!emailResult || emailResult.Data == null)
            {
                _log.LogError("Failed to decrypt user email: {ErrorMessage}", emailResult.Message);
                return emailResult.ConvertTo<User?>();
            }

            //--------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.DecryptString(encryptedUser.EncryptedDocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
            {
                _log.LogError("Failed to decrypt user document number: {ErrorMessage}", documentNumberResult.Message);
                return documentNumberResult.ConvertTo<User?>();
            }

            //------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.DecryptString(encryptedUser.EncryptedVerificationCode);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
            {
                _log.LogError("Failed to decrypt user verification code: {ErrorMessage}",
                    verificationCodeResult.Message);
                return verificationCodeResult.ConvertTo<User?>();
            }

            return new User
            {
                Id = encryptedUser.Id,
                Email = emailResult.Data,
                HashedPassword = encryptedUser.HashedPassword,
                DocumentType = encryptedUser.DocumentType,
                DocumentNumber = documentNumberResult.Data,
                Name = encryptedUser.Name,
                CreatedAt = encryptedUser.CreatedAt,
                UpdatedAt = encryptedUser.UpdatedAt,
                VerificationCode = verificationCodeResult.Data,
            };
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to decrypt user data");
            return new Result<User?>
            {
                Success = false,
                Code = "USER_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt user data",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    #endregion
}