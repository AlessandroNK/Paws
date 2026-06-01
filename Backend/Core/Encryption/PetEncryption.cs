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

    #region Encryption

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

            // Update the tracked entity
            trackedEntity.EncryptedName = nameResult.Data;
            trackedEntity.NameHash = nameHashResult.Data;
            trackedEntity.Species = pet.Species;
            trackedEntity.EncryptedBreed = breedResult.Data;
            trackedEntity.UpdatedAt = DateTime.UtcNow;
            trackedEntity.Status = pet.Status;

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

            // Create the new object
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
    /// Safely encrypts a shared invitation
    /// </summary>
    /// <param name="invitation"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static Result<EncryptedShareInvitation> EncryptShareInvitation(ShareInvitation invitation, ILogger logger)
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Email
            var newOwnerEmailResult = SecurityService.EncryptString(invitation.NewOwnerEmail);
            if (!newOwnerEmailResult || newOwnerEmailResult.Data == null)
                return newOwnerEmailResult.Log(logger).ConvertTo<EncryptedShareInvitation>();

            // ------------------------------------------------------------------------- Name
            var newNameResult = SecurityService.EncryptString(invitation.NewOwnerName);
            if (!newNameResult || newNameResult.Data == null)
                return newNameResult.Log(logger).ConvertTo<EncryptedShareInvitation>();

            // ------------------------------------------------------------------------- Nonce
            var nonceResult = SecurityService.EncryptString(invitation.Nonce);
            if (!nonceResult || nonceResult.Data == null)
                return nonceResult.Log(logger).ConvertTo<EncryptedShareInvitation>();

            var nonceHashResult = SecurityService.HashWithSalt(invitation.Nonce);
            if (!nonceHashResult || nonceHashResult.Data == null)
                return nonceHashResult.Log(logger).ConvertTo<EncryptedShareInvitation>();

            // create the element
            return new EncryptedShareInvitation
            {
                Id = invitation.Id,
                UserId = invitation.UserId,
                PetId = invitation.PetId,
                EncryptedNewOwnerEmail = newOwnerEmailResult.Data,
                EncryptedNewOwnerName = newNameResult.Data,
                NewOwnerHasAccount = invitation.NewOwnerHasAccount,
                Expiration = invitation.Expiration,
                EncryptedNonce = nonceResult.Data,
                NonceHash = nonceHashResult.Data,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt share invitation data");
            return new Result<EncryptedShareInvitation>
            {
                Success = false,
                Code = "SHARE_INVITATION_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt share invitation data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------

    #region Decryption

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

            // ------------------------------------------------------------------------- Share invitations
            var shareInvitations = DecryptShareInvitations(encryptedPet.ShareInvitations, logger);
            if (!shareInvitations) return shareInvitations.Log(logger).ConvertTo<Pet?>();

            // Create the element
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
                ShareInvitations = shareInvitations.Data ?? new List<ShareInvitation>()
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

    // -----------------------------------------------------------------------------------------------------------------
    public static Result<List<ShareInvitation>> DecryptShareInvitations(
        List<EncryptedShareInvitation> encryptedShareInvitations,
        ILogger logger
    )
    {
        try
        {
            var decryptedInvitations = new List<ShareInvitation>();
            foreach (var encryptedInvitation in encryptedShareInvitations)
            {
                var decryptedResult = DecryptShareInvitation(encryptedInvitation, logger);
                if (!decryptedResult || decryptedResult.Data == null)
                    return decryptedResult.Log(logger).ConvertTo<List<ShareInvitation>>();

                decryptedInvitations.Add(decryptedResult.Data);
            }

            return new Result<List<ShareInvitation>>
            {
                Success = true,
                Code = "SHARE_INVITATIONS_DECRYPTED",
                Status = 200,
                Message = "Share invitations decrypted successfully",
                TraceCode = FileCodes.CallerIC(),
                Data = decryptedInvitations,
                Returnable = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt share invitations");
            return new Result<List<ShareInvitation>>
            {
                Success = false,
                Code = "SHARE_INVITATIONS_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt share invitations",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static Result<ShareInvitation?> DecryptShareInvitation(EncryptedShareInvitation encryptedShareInvitation,
        ILogger logger)
    {
        try
        {
            // Decrypt elements
            // ------------------------------------------------------------------------- NewOwnerEmail
            var newOwnerEmailResult = SecurityService.DecryptString(encryptedShareInvitation.EncryptedNewOwnerEmail);
            if (!newOwnerEmailResult || newOwnerEmailResult.Data == null)
                return newOwnerEmailResult.Log(logger).ConvertTo<ShareInvitation?>();

            // ------------------------------------------------------------------------- NewOwnerName
            var newOwnerNameResult = SecurityService.DecryptString(encryptedShareInvitation.EncryptedNewOwnerName);
            if (!newOwnerNameResult || newOwnerNameResult.Data == null)
                return newOwnerNameResult.Log(logger).ConvertTo<ShareInvitation?>();

            // ------------------------------------------------------------------------- EncryptedNonce
            var encryptedNonceResult = SecurityService.DecryptString(encryptedShareInvitation.EncryptedNonce);
            if (!encryptedNonceResult || encryptedNonceResult.Data == null)
                return encryptedNonceResult.Log(logger).ConvertTo<ShareInvitation?>();

            // Create the object
            return new ShareInvitation
            {
                Id = encryptedShareInvitation.Id,
                UserId = encryptedShareInvitation.UserId,
                PetId = encryptedShareInvitation.PetId,
                NewOwnerEmail = newOwnerEmailResult.Data,
                NewOwnerName = newOwnerNameResult.Data,
                NewOwnerHasAccount = encryptedShareInvitation.NewOwnerHasAccount,
                Expiration = encryptedShareInvitation.Expiration,
                Nonce = encryptedNonceResult.Data,
                Status = encryptedShareInvitation.Status
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt share invitation data");
            return new Result<ShareInvitation?>
            {
                Success = false,
                Code = "SHARE_INVITATION_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt share invitation data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    #endregion
}