using Backend.Core.Internal;
using Backend.Core.Models.Pets;
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
            //------------------------------------------------------------------------- Name
            var nameResult = SecurityService.EncryptString(pet.Name);
            if (!nameResult || nameResult.Data == null)
                return nameResult.Log(logger).ConvertTo<EncryptedPet>();

            var nameHashResult = SecurityService.HashWithSalt(pet.Name);
            if (!nameHashResult || nameHashResult.Data == null)
                return nameHashResult.Log(logger).ConvertTo<EncryptedPet>();

            //------------------------------------------------------------------------- Breed
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

            if (!breedResult || breedResult.Data == null) return breedResult.Log(logger).ConvertTo<EncryptedPet>();

            return new EncryptedPet
            {
                Id = pet.Id,
                EncryptedName = nameResult.Data,
                NameHash = nameHashResult.Data,
                EncryptedBreed = breedResult.Data,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt,
                Status = pet.Status,
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
    /// <returns></returns>
    public static Result<Pet?> DecryptPet(EncryptedPet? encryptedPet, ILogger logger)
    {
        try
        {
            if (encryptedPet is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "ENCRYPTED_PET_NULL",
                    Status = 500,
                    Message = "no encrypted pet to decrypt",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = false
                };

            // Decrypt elements
            //------------------------------------------------------------------------- Name
            var nameResult = SecurityService.DecryptString(encryptedPet.EncryptedName);
            if (!nameResult || nameResult.Data == null)
                return nameResult.Log(logger).ConvertTo<Pet?>();

            //--------------------------------------------------------------- Breed
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

            return new Pet
            {
                Id = encryptedPet.Id,
                Name = nameResult.Data,
                Breed = breedResult.Data,
                CreatedAt = encryptedPet.CreatedAt,
                UpdatedAt = encryptedPet.UpdatedAt,
                Status = encryptedPet.Status,
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