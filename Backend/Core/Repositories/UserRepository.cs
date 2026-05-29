using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Result;
using Backend.Core.Models.User;
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
/// <remarks>FU01</remarks>
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
    private Result EncryptUserAndUpdateTrackedEntity(User user, EncryptedUser trackedEntity)
    {
        try
        {
            // Encrypt elements
            //---------------------------------------------------------------------- Password
            // This password is encrypted once, but if
            // a new request does not have password, then
            // we use the already hashed password
            var passwordResult = string.IsNullOrWhiteSpace(user.Password)
                ? new Result<string>
                {
                    Success = true,
                    Data = user.PasswordHash,
                    Code = "PASSWORD_NULL_OR_WHITESPACE",
                    Status = 200,
                    Message = "Password is null or whitespace, using existing hashed password",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.HashWithSalt(user.Password);
            if (!passwordResult || passwordResult.Data == null)
                return passwordResult.Log(_log);

            //------------------------------------------------------------------------- Email
            var emailResult = SecurityService.EncryptString(user.Email);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(_log);

            var emailHashResult = SecurityService.HashWithSalt(user.Email);
            if (!emailHashResult || emailHashResult.Data == null)
                return emailHashResult.Log(_log);

            //------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.EncryptString(user.DocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(_log);

            var documentHashResult = SecurityService.HashWithSalt(user.DocumentNumber);
            if (!documentHashResult || documentHashResult.Data == null)
                return documentHashResult.Log(_log);

            //------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.EncryptString(user.VerificationCode, true);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
                return verificationCodeResult.Log(_log);


            // Update the tracked entity
            trackedEntity.PasswordHash = passwordResult.Data;
            trackedEntity.EncryptedEmail = emailResult.Data;
            trackedEntity.EmailHash = emailHashResult.Data;
            trackedEntity.DocumentType = user.DocumentType;
            trackedEntity.EncryptedDocumentNumber = documentNumberResult.Data;
            trackedEntity.DocumentHash = documentHashResult.Data;
            trackedEntity.Name = user.Name;
            trackedEntity.UpdatedAt = user.UpdatedAt;
            trackedEntity.Status = user.Status;
            trackedEntity.EncryptedVerificationCode = verificationCodeResult.Data;

            return new Result
            {
                Success = true,
                Code = "USER_ENCRYPTED_AND_TRACKED_ENTITY_UPDATED",
                Status = 200,
                Message = "User encrypted and tracked entity updated successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to encrypt user data");
            return new Result
            {
                Success = false,
                Code = "USER_ENCRYPTION_FAILED",
                Status = 500,
                Message = "Failed to encrypt user data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

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
            // This password is encrypted once, but if
            // a new request does not have password, then
            // we use the already hashed password
            var passwordResult = string.IsNullOrWhiteSpace(user.Password)
                ? new Result<string>
                {
                    Success = true,
                    Data = user.PasswordHash,
                    Code = "PASSWORD_NULL_OR_WHITESPACE",
                    Status = 200,
                    Message = "Password is null or whitespace, using existing hashed password",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.HashWithSalt(user.Password);
            if (!passwordResult || passwordResult.Data == null)
                return passwordResult.Log(_log).ConvertTo<EncryptedUser>();

            //------------------------------------------------------------------------- Email
            var emailResult = SecurityService.EncryptString(user.Email);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(_log).ConvertTo<EncryptedUser>();

            var emailHashResult = SecurityService.HashWithSalt(user.Email);
            if (!emailHashResult || emailHashResult.Data == null)
                return emailHashResult.Log(_log).ConvertTo<EncryptedUser>();

            //------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.EncryptString(user.DocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(_log).ConvertTo<EncryptedUser>();

            var documentHashResult = SecurityService.HashWithSalt(user.DocumentNumber);
            if (!documentHashResult || documentHashResult.Data == null)
                return documentHashResult.Log(_log).ConvertTo<EncryptedUser>();

            //------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.EncryptString(user.VerificationCode, true);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
                return verificationCodeResult.Log(_log).ConvertTo<EncryptedUser>();

            return new EncryptedUser
            {
                Id = user.Id,
                PasswordHash = passwordResult.Data,
                EncryptedEmail = emailResult.Data,
                EmailHash = emailHashResult.Data,
                DocumentType = user.DocumentType,
                EncryptedDocumentNumber = documentNumberResult.Data,
                DocumentHash = documentHashResult.Data,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Status = user.Status,
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
                TraceCode = FileCodes.CallerIC(),
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
            if (encryptedUser is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "ENCRYPTED_USER_NULL",
                    Status = 500,
                    Message = "no encrypted user to decrypt",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = false
                };

            // Decrypt elements
            //------------------------------------------------------------------------- Email
            var emailResult = SecurityService.DecryptString(encryptedUser.EncryptedEmail);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(_log).ConvertTo<User?>();

            //--------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.DecryptString(encryptedUser.EncryptedDocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(_log).ConvertTo<User?>();

            //------------------------------------------------------------- Verification Code
            var verificationCodeResult = string.IsNullOrWhiteSpace(encryptedUser.EncryptedVerificationCode)
                ? new Result<string>
                {
                    Success = true,
                    Status = 200,
                    Message = "Verification code is null or whitespace, returning empty string",
                    Code = "VERIFICATION_CODE_NULL_OR_WHITESPACE",
                    Data = string.Empty,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.DecryptString(encryptedUser.EncryptedVerificationCode);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
                return verificationCodeResult.Log(_log).ConvertTo<User?>();

            return new User
            {
                Id = encryptedUser.Id,
                Email = emailResult.Data,
                Status = encryptedUser.Status,
                PasswordHash = encryptedUser.PasswordHash,
                DocumentType = encryptedUser.DocumentType,
                DocumentNumber = documentNumberResult.Data,
                Name = encryptedUser.Name,
                CreatedAt = encryptedUser.CreatedAt,
                UpdatedAt = encryptedUser.UpdatedAt,
                VerificationCode = verificationCodeResult?.Data,
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
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------

    #region DB

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        bool excludeInactive = true,
        bool excludeBanned = true
    )
    {
        // Encrypt user data to find it in the db
        var hashedEmailResult = SecurityService.HashWithSalt(email);
        if (!hashedEmailResult || hashedEmailResult.Data == null)
            return hashedEmailResult.Log(_log).ConvertTo<User?>();

        // Find the user
        var query = _dbContext.EncryptedUsers
            .Where(u => u.EmailHash == hashedEmailResult.Data);

        if (excludeInactive) query = query.Where(u => u.Status != UserStatus.Inactive);
        if (excludeBanned) query = query.Where(u => u.Status != UserStatus.Banned);

        query = query
            // .Include(u => u.DocumentType)
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
                TraceCode = $"{FileCodes.CallerIC()}",
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
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <returns>The user if any</returns>
    public async Task<Result<User?>> GetByDocumentAsync(
        string document,
        bool excludeInactive = true,
        bool excludeBanned = true
    )
    {
        // Encrypt user data to find it in the db
        var hashedDocumentResult = SecurityService.HashWithSalt(document);
        if (!hashedDocumentResult || hashedDocumentResult.Data == null)
        {
            _log.LogError("Failed to encrypt document for user lookup: {ErrorMessage}", hashedDocumentResult.Message);
            return hashedDocumentResult.ConvertTo<User?>();
        }

        // Find the user
        var query = _dbContext.EncryptedUsers.AsQueryable()
            .Where(u => u.DocumentHash == hashedDocumentResult.Data);

        if (excludeInactive) query = query.Where(u => u.Status != UserStatus.Inactive);
        if (excludeBanned) query = query.Where(u => u.Status != UserStatus.Banned);

        query = query
            // .Include(u => u.DocumentType)
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
                TraceCode = $"{FileCodes.CallerIC()}",
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
    public async Task<Result<User?>> AddAsync(User user)
    {
        // Encrypt user data
        var result = EncryptUser(user);
        if (!result || result.Data is null) return result.ConvertTo<User?>();

        // Save the user
        _dbContext.EncryptedUsers.Add(result.Data);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved < 0)
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_CREATING_USER",
                Status = 500,
                Message = "An error occurred while creating the user",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Get the user back from the db
        var getUserResult = await GetByEmailAsync(user.Email, false);
        if (!getUserResult || getUserResult.Data == null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_CREATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "User created but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        user = getUserResult.Data;
        return new Result<User?>
        {
            Success = true,
            Code = "USER_CREATED",
            Status = 201,
            Message = "User created successfully",
            Data = user,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates an existing user. It takes the device id from the header and the sign up request from the body. It returns
    /// an instance of the updated user
    ///
    /// </summary>
    /// <param name="user">the <see cref="User"/> to update</param>
    /// <returns>The <see cref="User"/></returns>
    public async Task<Result<User?>> UpdateAsync(User user)
    {
        if (user.Id <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_ID_NOT_PROVIDED",
                Status = 400,
                Message = "User id not provided for update",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // First get the existing encrypted user from the database
        var existingEncryptedUser = await _dbContext.EncryptedUsers
            .FirstOrDefaultAsync(eu => eu.Id == user.Id);

        if (existingEncryptedUser == null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Message = "User not found",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Update the tracked entity with new encrypted values
        var updateResult = EncryptUserAndUpdateTrackedEntity(user, existingEncryptedUser);
        if (!updateResult)
            return updateResult.ConvertTo<User?>();

        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0) // Note: changed from < 0 to <= 0
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_UPDATING_USER",
                Status = 500,
                Message = "An error occurred while updating the user",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        var getUserResult = await GetByEmailAsync(user.Email, false);
        if (!getUserResult || getUserResult.Data == null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_UPDATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "User updated but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<User?>
        {
            Success = true,
            Code = "USER_UPDATED",
            Status = 200,
            Message = "User updated successfully",
            Data = getUserResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Helpers

    #endregion
}