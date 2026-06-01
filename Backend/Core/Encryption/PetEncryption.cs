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
    /// Safely encrypts an ownership invitation
    /// </summary>
    /// <param name="invitation"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static Result<EncryptedOwnershipInvitation> EncryptOwnershipInvitation(OwnershipInvitation invitation,
        ILogger logger)
    {
        try
        {
            // Encrypt elements
            // ------------------------------------------------------------------------- Email
            var newOwnerEmailResult = SecurityService.EncryptString(invitation.NewOwnerEmail);
            if (!newOwnerEmailResult || newOwnerEmailResult.Data == null)
                return newOwnerEmailResult.Log(logger).ConvertTo<EncryptedOwnershipInvitation>();

            // ------------------------------------------------------------------------- Name
            var newNameResult = SecurityService.EncryptString(invitation.NewOwnerName);
            if (!newNameResult || newNameResult.Data == null)
                return newNameResult.Log(logger).ConvertTo<EncryptedOwnershipInvitation>();

            // ------------------------------------------------------------------------- InvitationCode
            var invitationCodeResult = SecurityService.EncryptString(invitation.InvitationCode);
            if (!invitationCodeResult || invitationCodeResult.Data == null)
                return invitationCodeResult.Log(logger).ConvertTo<EncryptedOwnershipInvitation>();

            var invitationCodeHashResult = SecurityService.HashWithSalt(invitation.InvitationCode);
            if (!invitationCodeHashResult || invitationCodeHashResult.Data == null)
                return invitationCodeHashResult.Log(logger).ConvertTo<EncryptedOwnershipInvitation>();

            // create the element
            return new EncryptedOwnershipInvitation
            {
                Id = invitation.Id,
                EncryptedUserId = invitation.UserId,
                EncryptedPetId = invitation.PetId,
                EncryptedNewOwnerEmail = newOwnerEmailResult.Data,
                EncryptedNewOwnerName = newNameResult.Data,
                NewOwnerHasAccount = invitation.NewOwnerHasAccount,
                Expiration = invitation.Expiration,
                EncryptedInvitationCode = invitationCodeResult.Data,
                InvitationCodeHash = invitationCodeHashResult.Data,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt ownership invitation data");
            return new Result<EncryptedOwnershipInvitation>
            {
                Success = false,
                Code = "OWNERSHIP_INVITATION_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt ownership invitation data",
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
    /// <param name="decryptOwnershipInvitations">A flag to indicate if we should decrypt ownership invitations or not.
    /// We can set it to false to avoid unwanted recursiveness</param>
    /// <returns></returns>
    public static Result<Pet?> DecryptPet(
        EncryptedPet encryptedPet,
        ILogger logger,
        bool decryptUserPets = true,
        bool decryptOwnershipInvitations = true
    )
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

            // ------------------------------------------------------------------------- ownership invitations
            var ownershipInvitations = decryptOwnershipInvitations
                ? DecryptOwnershipInvitations(encryptedPet.OwnershipInvitations, logger)
                : new Result<List<OwnershipInvitation>>
                {
                    Success = true,
                    Status = 200,
                    Code = "OWNERSHIP_INVITATIONS_DECRYPTED",
                    Message = "Ownership invitations decrypted successfully",
                    TraceCode = FileCodes.CallerIC(),
                    Data = new List<OwnershipInvitation>(),
                    Returnable = true
                };
            if (!ownershipInvitations) return ownershipInvitations.Log(logger).ConvertTo<Pet?>();

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
                OwnershipInvitations = ownershipInvitations.Data ?? new List<OwnershipInvitation>()
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
    public static Result<List<OwnershipInvitation>> DecryptOwnershipInvitations(
        List<EncryptedOwnershipInvitation> encryptedOwnershipInvitations,
        ILogger logger
    )
    {
        try
        {
            var decryptedInvitations = new List<OwnershipInvitation>();
            foreach (var encryptedInvitation in encryptedOwnershipInvitations)
            {
                var decryptedResult = DecryptOwnershipInvitation(encryptedInvitation, logger);
                if (!decryptedResult || decryptedResult.Data == null)
                    return decryptedResult.Log(logger).ConvertTo<List<OwnershipInvitation>>();

                decryptedInvitations.Add(decryptedResult.Data);
            }

            return new Result<List<OwnershipInvitation>>
            {
                Success = true,
                Code = "OWNERSHIP_INVITATIONS_DECRYPTED",
                Status = 200,
                Message = "Ownership invitations decrypted successfully",
                TraceCode = FileCodes.CallerIC(),
                Data = decryptedInvitations,
                Returnable = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt ownership invitations");
            return new Result<List<OwnershipInvitation>>
            {
                Success = false,
                Code = "OWNERSHIP_INVITATIONS_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt ownership invitations",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static Result<OwnershipInvitation?> DecryptOwnershipInvitation(
        EncryptedOwnershipInvitation encryptedOwnershipInvitation,
        ILogger logger
    )
    {
        try
        {
            // Decrypt elements
            // ------------------------------------------------------------------------- NewOwnerEmail
            var newOwnerEmailResult =
                SecurityService.DecryptString(encryptedOwnershipInvitation.EncryptedNewOwnerEmail);
            if (!newOwnerEmailResult || newOwnerEmailResult.Data == null)
                return newOwnerEmailResult.Log(logger).ConvertTo<OwnershipInvitation?>();

            // ------------------------------------------------------------------------- NewOwnerName
            var newOwnerNameResult = SecurityService.DecryptString(encryptedOwnershipInvitation.EncryptedNewOwnerName);
            if (!newOwnerNameResult || newOwnerNameResult.Data == null)
                return newOwnerNameResult.Log(logger).ConvertTo<OwnershipInvitation?>();

            // ------------------------------------------------------------------------- EncryptedInvitationCode
            var encryptedInvitationCodeResult =
                SecurityService.DecryptString(encryptedOwnershipInvitation.EncryptedInvitationCode);
            if (!encryptedInvitationCodeResult || encryptedInvitationCodeResult.Data == null)
                return encryptedInvitationCodeResult.Log(logger).ConvertTo<OwnershipInvitation?>();

            // ------------------------------------------------------------------------- Encrypted user
            var encryptedUserResult = UserEncryption.DecryptUser(
                encryptedOwnershipInvitation.EncryptedUser,
                logger,
                false
            );
            if (!encryptedUserResult || encryptedUserResult.Data == null)
                return encryptedUserResult.Log(logger).ConvertTo<OwnershipInvitation?>();

            // ------------------------------------------------------------------------- EncryptedPet
            var encryptedPetResult = DecryptPet(
                encryptedOwnershipInvitation.EncryptedPet,
                logger,
                false,
                false
            );
            if (!encryptedPetResult || encryptedPetResult.Data == null)
                return encryptedPetResult.Log(logger).ConvertTo<OwnershipInvitation?>();

            // Create the object
            return new OwnershipInvitation
            {
                Id = encryptedOwnershipInvitation.Id,
                UserId = encryptedOwnershipInvitation.EncryptedUserId,
                User = encryptedUserResult.Data,
                PetId = encryptedOwnershipInvitation.EncryptedPetId,
                Pet = encryptedPetResult.Data,
                NewOwnerEmail = newOwnerEmailResult.Data,
                NewOwnerName = newOwnerNameResult.Data,
                NewOwnerHasAccount = encryptedOwnershipInvitation.NewOwnerHasAccount,
                Expiration = encryptedOwnershipInvitation.Expiration,
                InvitationCode = encryptedInvitationCodeResult.Data,
                Status = encryptedOwnershipInvitation.Status
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to decrypt ownership invitation data");
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "OWNERSHIP_INVITATION_DECRYPTION_FAILED",
                Status = 500,
                Message = "Failed to decrypt ownership invitation data",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }
    }

    #endregion
}