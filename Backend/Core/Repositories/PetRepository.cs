using Backend.Core.Data;
using Backend.Core.Encryption;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

public class PetRepository(
    ApplicationDbContext dbContext,
    ILogger<PetRepository> logger
) : IPetRepository
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
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<PetRepository> _logger = logger;


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------
    //                                                                                                   Private Methods


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new pet. It returns an <see cref="Result{Pet}"/> indicating the result of the operation and including
    /// the pet if it was success.
    /// </summary>
    /// <param name="pet">The pet to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the creation was successful</returns>
    public async Task<Result<Pet?>> AddAsync(Pet pet)
    {
        // Encrypt pet data
        var result = PetEncryption.EncryptPet(pet, _logger);
        if (!result || result.Data is null) return result.ConvertTo<Pet?>();

        // Save the pet
        _dbContext.EncryptedPets.Add(result.Data);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_CREATING_PET",
                Status = 500,
                Message = "An error occurred while creating the pet",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Get the pet back from the db
        var getPetResult = await GetByIdAsync(result.Data.Id);
        if (!getPetResult || getPetResult.Data == null)
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_CREATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "Pet created but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        pet = getPetResult.Data;
        return new Result<Pet?>
        {
            Success = true,
            Code = "PET_CREATED",
            Status = 201,
            Message = "Pet created successfully",
            Data = pet,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a pet by its ID.
    /// </summary>
    /// <param name="id">The ID of the pet to retrieve</param>
    /// <param name="excludeInactive">Whether to exclude inactive pets</param>
    /// <param name="excludeBanned">Whether to exclude banned pets</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public async Task<Result<Pet?>> GetByIdAsync(int id, bool excludeInactive = true, bool excludeBanned = true)
    {
        if (id <= 0)
            return new Result<Pet?>
            {
                Success = false,
                Code = "INVALID_PET_ID",
                Status = 400,
                Message = "The provided pet ID is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the pet
        var query = _dbContext.EncryptedPets
            .Where(p => p.Id == id);

        if (excludeInactive) query = query.Where(p => p.Status != PetStatus.Inactive);
        if (excludeBanned) query = query.Where(p => p.Status != PetStatus.Banned);

        query = query
            .Include(p => p.UserPets)
            .ThenInclude(up => up.EncryptedUser)
            .AsSplitQuery();

        // Execute query
        var encryptedPet = await query.FirstOrDefaultAsync();
        if (encryptedPet is null)
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_NOT_FOUND",
                Status = 404,
                Message = "No pet found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return pet
        return PetEncryption.DecryptPet(encryptedPet, _logger);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates a pet in the system. It takes the pet data and returns a result with the updated pet or an error if
    /// something went wrong.
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    public async Task<Result<Pet?>> UpdateAsync(Pet pet)
    {
        if (pet.Id <= 0)
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_ID_NOT_PROVIDED",
                Status = 400,
                Message = "Pet id not provided for update",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // First get the existing encrypted pet from the database
        var existingEncryptedPet = await _dbContext.EncryptedPets
            .FirstOrDefaultAsync(ep => ep.Id == pet.Id);

        if (existingEncryptedPet == null)
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_NOT_FOUND",
                Status = 404,
                Message = "pet not found",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Update the tracked entity with new encrypted values
        var updateResult = PetEncryption.EncryptPetAndUpdateTrackedEntity(pet, existingEncryptedPet, _logger);
        if (!updateResult) return updateResult.ConvertTo<Pet?>();

        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_UPDATING_PET",
                Status = 500,
                Message = "An error occurred while updating the pet",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        var getPetResult = await GetByIdAsync(pet.Id, excludeInactive: false, excludeBanned: false);
        if (!getPetResult || getPetResult.Data == null)
            return new Result<Pet?>
            {
                Success = false,
                Code = "PET_UPDATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "Pet updated but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<Pet?>
        {
            Success = true,
            Code = "PET_UPDATED",
            Status = 200,
            Message = "Pet updated successfully",
            Data = getPetResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
         };
    }
}