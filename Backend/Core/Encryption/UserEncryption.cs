using Backend.Core.Internal;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Services;

namespace Backend.Core.Encryption;

public static class UserEncryption
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


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
    public static Result EncryptUserAndUpdateTrackedEntity(
        User user,
        EncryptedUser trackedEntity,
        ILogger logger
    )
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Password
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
                return passwordResult.Log(logger);

            // ------------------------------------------------------------------------- Email
            var emailResult = SecurityService.EncryptString(user.Email);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(logger);

            var emailHashResult = SecurityService.HashWithSalt(user.Email);
            if (!emailHashResult || emailHashResult.Data == null)
                return emailHashResult.Log(logger);

            // ------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.EncryptString(user.DocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(logger);

            var documentHashResult = SecurityService.HashWithSalt(user.DocumentNumber);
            if (!documentHashResult || documentHashResult.Data == null)
                return documentHashResult.Log(logger);

            // ------------------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.EncryptString(user.VerificationCode, true);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
                return verificationCodeResult.Log(logger);


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
            logger.LogError(e, "Failed to encrypt user data");
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
    /// <param name="logger">The logger so this function can log errors</param>
    /// <returns></returns>
    public static Result<EncryptedUser> EncryptUser(User user, ILogger logger)
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Password
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
                return passwordResult.Log(logger).ConvertTo<EncryptedUser>();

            // ------------------------------------------------------------------------- Email
            var emailResult = SecurityService.EncryptString(user.Email);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(logger).ConvertTo<EncryptedUser>();

            var emailHashResult = SecurityService.HashWithSalt(user.Email);
            if (!emailHashResult || emailHashResult.Data == null)
                return emailHashResult.Log(logger).ConvertTo<EncryptedUser>();

            // ------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.EncryptString(user.DocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(logger).ConvertTo<EncryptedUser>();

            var documentHashResult = SecurityService.HashWithSalt(user.DocumentNumber);
            if (!documentHashResult || documentHashResult.Data == null)
                return documentHashResult.Log(logger).ConvertTo<EncryptedUser>();

            // ------------------------------------------------------------------------- Verification Code
            var verificationCodeResult = SecurityService.EncryptString(user.VerificationCode, true);
            if (!verificationCodeResult || verificationCodeResult.Data == null)
                return verificationCodeResult.Log(logger).ConvertTo<EncryptedUser>();

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
            logger.LogError(e, "Failed to encrypt user data");
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
    /// <param name="logger">The logger so this function can log errors</param>
    /// <param name="decryptUserPets">A flag to indicate if we should decrypt user pets or not. We can set it to false
    /// to avoid unwanted recursiveness</param>
    /// <returns></returns>
    public static Result<User?> DecryptUser(EncryptedUser encryptedUser, ILogger logger, bool decryptUserPets = true)
    {
        try
        {
            // Decrypt elements
            // ------------------------------------------------------------------------- UserPets
            var userPets = decryptUserPets
                ? UserPetsEncryption.DecryptUserPets(encryptedUser.UserPets, logger)
                : new Result<List<UserPet>>
                {
                    Success = true,
                    Status = 200,
                    Code = "USER_PETS_DECRYPTED",
                    Message = "User pets decrypted successfully",
                    TraceCode = FileCodes.CallerIC(),
                    Data = new List<UserPet>()
                };

            // ------------------------------------------------------------------------- Email
            var emailResult = SecurityService.DecryptString(encryptedUser.EncryptedEmail);
            if (!emailResult || emailResult.Data == null)
                return emailResult.Log(logger).ConvertTo<User?>();

            // ------------------------------------------------------------------------- Document Number
            var documentNumberResult = SecurityService.DecryptString(encryptedUser.EncryptedDocumentNumber);
            if (!documentNumberResult || documentNumberResult.Data == null)
                return documentNumberResult.Log(logger).ConvertTo<User?>();

            // ------------------------------------------------------------------------- Verification Code
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
                return verificationCodeResult.Log(logger).ConvertTo<User?>();

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
                VerificationCode = verificationCodeResult.Data ?? string.Empty,
                UserPets = userPets.Data ?? new List<UserPet>()
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt user data");
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
}