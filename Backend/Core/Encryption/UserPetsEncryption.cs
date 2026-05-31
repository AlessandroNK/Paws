using Backend.Core.Internal;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;

namespace Backend.Core.Encryption;

public static class UserPetsEncryption
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
    /// Securely decrypts pets inside userPet list element
    /// </summary>
    /// <param name="encryptedUserPets">A list of <see cref="UserPet"/> to decrypt</param>
    /// <param name="logger"></param>
    /// <remarks>To avoid unwanted recursiveness, this function does not decrypt UserPets inside userPet's user and Pet</remarks>
    /// <returns></returns>
    public static Result<List<UserPet>> DecryptUserPets(List<EncryptedUserPet> encryptedUserPets, ILogger logger)
    {
        List<UserPet> userPets = new();

        foreach (var userPet in encryptedUserPets)
        {
            var result = DecryptUserPet(userPet, logger);
            if (!result || result.Data == null) continue;
            userPets.Add(result.Data);
        }

        return new Result<List<UserPet>>
        {
            Success = true,
            Status = 200,
            Code = "USER_PETS_DECRYPTED",
            Message = "User pets decrypted successfully",
            TraceCode = FileCodes.CallerIC(),
            Data = userPets
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Securely decrypts pet inside userPet element
    /// </summary>
    /// <param name="encryptedUserPet"></param>
    /// <param name="logger"></param>
    /// <remarks>To avoid unwanted recursiveness, this function does not decrypt UserPets inside userPet's user and Pet</remarks>
    /// <returns></returns>
    public static Result<UserPet> DecryptUserPet(EncryptedUserPet encryptedUserPet, ILogger logger)
    {
        try
        {
            // ------------------------------------------------------------------------- User
            var userResult = Result<User?>.GetDefaultSuccess();
            if (encryptedUserPet.EncryptedUser is not null)
            {
                userResult = UserEncryption.DecryptUser(encryptedUserPet.EncryptedUser, logger, false);
                if (!userResult || userResult.Data == null)
                    return userResult.Log(logger).ConvertTo<UserPet>();
            }

            // ------------------------------------------------------------------------- Pets
            var petResult = Result<Pet?>.GetDefaultSuccess();
            if (encryptedUserPet.EncryptedPet is not null)
            {
                petResult = PetEncryption.DecryptPet(encryptedUserPet.EncryptedPet, logger, false);
                if (!petResult || petResult.Data == null)
                    return petResult.Log(logger).ConvertTo<UserPet>();
            }

            return new UserPet
            {
                Id = encryptedUserPet.Id,
                UserId = encryptedUserPet.EncryptedUserId,
                PetId = encryptedUserPet.EncryptedPetId,
                CreatedAt = encryptedUserPet.CreatedAt,
                UpdatedAt = encryptedUserPet.UpdatedAt,
                Status = encryptedUserPet.Status,
                Pet = petResult.Data,
                User = userResult.Data
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt user pet data");
            return new Result<UserPet>
            {
                Success = false,
                Code = "USER_PET_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt user pet data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }
}