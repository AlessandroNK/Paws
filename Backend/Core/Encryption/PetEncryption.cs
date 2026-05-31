using Backend.Core.Internal;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Services;

namespace Backend.Core.Encryption;

public static class PetEncryption
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
    public static Result EncryptPetAndUpdateTrackedEntity(
        Pet pet,
        EncryptedPet trackedEntity,
        ILogger logger
    )
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Name
            var nameResult = SecurityService.EncryptString(pet.Name);
            if (!nameResult || nameResult.Data == null)
                return nameResult.Log(logger).ConvertTo<EncryptedPet>();

            var nameHashResult = SecurityService.HashWithSalt(pet.Name);
            if (!nameHashResult || nameHashResult.Data == null)
                return nameHashResult.Log(logger).ConvertTo<EncryptedPet>();

            // ------------------------------------------------------------------------- Breed
            var breedResult = string.IsNullOrWhiteSpace(pet.Breed)
                ? new Result<string>
                {
                    Success = true,
                    Code = "BREED",
                    Status = 200,
                    Message = "No breed provided",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.EncryptString(pet.Breed);
            if (!breedResult) return breedResult.Log(logger).ConvertTo<EncryptedPet>();

            // ------------------------------------------------------------------------- ShareCode
            var shareCodeResult = string.IsNullOrWhiteSpace(pet.ShareCode)
                ? new Result<string>
                {
                    Success = true,
                    Code = "SHARE_CODE",
                    Status = 200,
                    Message = "No share provided",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.EncryptString(pet.ShareCode);
            if (!shareCodeResult) return shareCodeResult.Log(logger).ConvertTo<EncryptedPet>();

            // Update the tracked entity
            trackedEntity.EncryptedName = nameResult.Data;
            trackedEntity.NameHash = nameHashResult.Data;
            trackedEntity.Species = pet.Species;
            trackedEntity.EncryptedBreed = breedResult.Data;
            trackedEntity.UpdatedAt = DateTime.UtcNow;
            trackedEntity.Status = pet.Status;
            trackedEntity.EncryptedShareCode = shareCodeResult.Data;
            trackedEntity.ShareCodeExpiration = pet.ShareCodeExpiration;

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
    /// <param name="pet"></param>
    /// <param name="logger">A logger so this function can logg information</param>
    /// <returns></returns>
    public static Result<EncryptedPet> EncryptPet(Pet pet, ILogger logger)
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Name
            var nameResult = SecurityService.EncryptString(pet.Name);
            if (!nameResult || nameResult.Data == null)
                return nameResult.Log(logger).ConvertTo<EncryptedPet>();

            var nameHashResult = SecurityService.HashWithSalt(pet.Name);
            if (!nameHashResult || nameHashResult.Data == null)
                return nameHashResult.Log(logger).ConvertTo<EncryptedPet>();

            // ------------------------------------------------------------------------- Breed
            var breedResult = string.IsNullOrWhiteSpace(pet.Breed)
                ? new Result<string>
                {
                    Success = true,
                    Code = "BREED",
                    Status = 200,
                    Message = "No breed provided",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.EncryptString(pet.Breed);
            if (!breedResult) return breedResult.Log(logger).ConvertTo<EncryptedPet>();


            // ------------------------------------------------------------------------- ShareCode
            var shareCodeResult = string.IsNullOrWhiteSpace(pet.ShareCode)
                ? new Result<string>
                {
                    Success = true,
                    Code = "SHARE_CODE",
                    Status = 200,
                    Message = "No share provided",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.EncryptString(pet.ShareCode);
            if (!shareCodeResult) return shareCodeResult.Log(logger).ConvertTo<EncryptedPet>();

            return new EncryptedPet
            {
                Id = pet.Id,
                EncryptedName = nameResult.Data,
                NameHash = nameHashResult.Data,
                Species = pet.Species,
                EncryptedBreed = breedResult.Data,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt,
                Status = pet.Status,
                EncryptedShareCode = shareCodeResult.Data,
                ShareCodeExpiration = pet.ShareCodeExpiration
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to encrypt pet data");
            return new Result<EncryptedPet>
            {
                Success = false,
                Code = "PET_ENCRYPTION_FAILED",
                Status = 500,
                Message = "Failed to encrypt pet data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Decrypts sensitive data so we can use it and return it to the frontend
    /// </summary>
    /// <param name="encryptedPet"></param>
    /// <param name="logger">A logger so this function can logg information</param>
    /// <param name="decryptUserPets">A flag to indicate if we should decrypt user pets or not. We can set it to false
    /// to avoid unwanted recursiveness</param>
    /// <returns></returns>
    public static Result<Pet?> DecryptPet(EncryptedPet encryptedPet, ILogger logger, bool decryptUserPets = true)
    {
        try
        {
            // Decrypt elements
            // ------------------------------------------------------------------------- UserPets
            var userPets = decryptUserPets
                ? UserPetsEncryption.DecryptUserPets(encryptedPet.UserPets, logger)
                : new Result<List<UserPet>>
            {
                Success = true,
                Status = 200,
                Code = "USER_PETS_DECRYPTED",
                Message = "User pets decrypted successfully",
                TraceCode = FileCodes.CallerIC(),
                Data = new List<UserPet>()
            };

            // ------------------------------------------------------------------------- Name
            var nameResult = SecurityService.DecryptString(encryptedPet.EncryptedName);
            if (!nameResult || nameResult.Data == null)
                return nameResult.Log(logger).ConvertTo<Pet?>();

            // ------------------------------------------------------------------------- Breed
            var breedResult = string.IsNullOrWhiteSpace(encryptedPet.EncryptedBreed)
                ? new Result<string>
                {
                    Success = true,
                    Status = 200,
                    Message = "Breed is null or whitespace, returning empty string",
                    Code = "BREED_NULL_OR_WHITESPACE",
                    Data = string.Empty,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.DecryptString(encryptedPet.EncryptedBreed);
            if (!breedResult || breedResult.Data == null)
                return breedResult.Log(logger).ConvertTo<Pet?>();

            // ------------------------------------------------------------------------- ShareCode
            var shareCodeResult = string.IsNullOrWhiteSpace(encryptedPet.EncryptedShareCode)
                ? new Result<string>
                {
                    Success = true,
                    Status = 200,
                    Message = "Share code is null or whitespace, returning empty string",
                    Code = "SHARE_CODE_NULL_OR_WHITESPACE",
                    Data = string.Empty,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : SecurityService.DecryptString(encryptedPet.EncryptedShareCode);
            if (!shareCodeResult || shareCodeResult.Data == null)
                return shareCodeResult.Log(logger).ConvertTo<Pet?>();

            return new Pet
            {
                Id = encryptedPet.Id,
                Name = nameResult.Data,
                Breed = breedResult.Data,
                CreatedAt = encryptedPet.CreatedAt,
                UpdatedAt = encryptedPet.UpdatedAt,
                Status = encryptedPet.Status,
                Species = encryptedPet.Species,
                UserPets = userPets.Data ?? new List<UserPet>(),
                ShareCode = shareCodeResult.Data,
                ShareCodeExpiration = encryptedPet.ShareCodeExpiration
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt pet data");
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt pet data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }
}