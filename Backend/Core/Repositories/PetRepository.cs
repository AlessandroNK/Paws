using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

/// <remarks>FP03</remarks>
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
    private static IQueryable<OwnershipInvitation> ApplyStatusFilters(
        IQueryable<OwnershipInvitation> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(i => i.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(i => i.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(i => i.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(i => i.Status != EntityStatus.Banned);
        if (!filters.IncludeArchived) query = query.Where(i => i.Status != EntityStatus.Archived);
        if (!filters.IncludeToDelete) query = query.Where(i => i.Status != EntityStatus.ToDelete);

        return query;
    }

    // -----------------------------------------------------------------------------------------------------------------
    private static IQueryable<Pet> ApplyStatusFilters(
        IQueryable<Pet> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(i => i.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(i => i.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(i => i.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(i => i.Status != EntityStatus.Banned);
        if (!filters.IncludeArchived) query = query.Where(i => i.Status != EntityStatus.Archived);
        if (!filters.IncludeToDelete) query = query.Where(i => i.Status != EntityStatus.ToDelete);

        return query;
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new pet. It returns an <see cref="Result{Pet}"/> indicating the result of the operation and including
    /// the pet if it was success.
    /// </summary>
    /// <param name="pet">The pet to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the creation was successful</returns>
    public async Task<Result<Pet?>> AddAsync(Pet pet)
    {
        // Save the pet
        _dbContext.Pets.Add(pet);
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
        var getPetResult = await GetByIdAsync(pet.Id);
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

        return new Result<Pet?>
        {
            Success = true,
            Code = "PET_CREATED",
            Status = 201,
            Message = "Pet created successfully",
            Data = getPetResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a pet by its ID.
    /// </summary>
    /// <param name="id">The ID of the pet to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeUsers">Whether to include the user data in the query</param>
    /// <param name="includeOwnerInvitations">Whether to include the ownership invitations in the query</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public async Task<Result<Pet?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUsers = false,
        bool includeOwnerInvitations = false
    )
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
        var query = _dbContext.Pets
            .Where(p => p.Id == id);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeUsers)
            query = query
                .Include(p => p.UserPets)
                .ThenInclude(up => up.User);

        if (includeOwnerInvitations) query = query.Include(p => p.OwnershipInvitations);

        query = query.AsSplitQuery();

        // Execute query
        var pet = await query.FirstOrDefaultAsync();
        return pet is null
            ? new Result<Pet?>
            {
                Success = false,
                Code = "PET_NOT_FOUND",
                Status = 404,
                Message = "No pet found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            }
            : new Result<Pet?>
            {
                Success = true,
                Code = "PET_FOUND",
                Status = 200,
                Message = "Pet found successfully",
                Data = pet,
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };
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

        var getPetResult = await GetByIdAsync(pet.Id);
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


    #region OwnershipInvitation

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously adds an ownership invitation
    /// </summary>
    /// <param name="invitation"></param>
    /// <returns></returns>
    public async Task<Result<OwnershipInvitation?>> AddOwnershipInvitationAsync(OwnershipInvitation invitation)
    {
        if (invitation.PetId <= 0)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "PET_ID_NOT_PROVIDED",
                Status = 400,
                Message = "Pet id not provided for ownership invitation",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        if (invitation.UserId <= 0)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "USER_ID_NOT_PROVIDED",
                Status = 400,
                Message = "User id not provided for ownership invitation",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Add the entity
        _dbContext.OwnershipInvitations.Add(invitation);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "ERROR_CREATING_OWNERSHIP_INVITATION",
                Status = 500,
                Message = "An error occurred while creating the ownership invitation",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Get it again from the DB
        var getResult = await GetOwnershipInvitationByIdAsync(invitation.Id);
        if (!getResult || getResult.Data is null)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "OWNERSHIP_INVITATION_CREATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "Ownership invitation created but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<OwnershipInvitation?>
        {
            Success = true,
            Code = "OWNERSHIP_INVITATION_CREATED",
            Status = 201,
            Message = "Ownership invitation created successfully",
            Data = getResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously gets an ownership invitation by its ID, applying optional status filters to the query.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="filters"></param>
    /// <param name="includeUsers"></param>
    /// <param name="includePet"></param>
    /// <returns></returns>
    public async Task<Result<OwnershipInvitation?>> GetOwnershipInvitationByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUsers = true,
        bool includePet = true
    )
    {
        if (id <= 0)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "INVALID_OWNERSHIP_INVITATION_ID",
                Status = 400,
                Message = "The provided ownership invitation ID is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the pet
        var query = _dbContext.OwnershipInvitations
            .Where(i => i.Id == id);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeUsers) query = query.Include(i => i.User);
        if (includePet) query = query.Include(i => i.Pet);

        query = query
            .AsSplitQuery();

        // Execute query
        var invitation = await query.FirstOrDefaultAsync();
        if (invitation is null)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "SHARE_INVITATION_NOT_FOUND",
                Status = 404,
                Message = "No ownership invitation found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<OwnershipInvitation?>
        {
            Success = true,
            Code = "OWNERSHIP_INVITATION_FOUND",
            Status = 200,
            Message = "Ownership invitation found successfully",
            Data = invitation,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets an ownership invitation by its invitation code, applying optional status filters to the query. The invitation codeis hashed and
    /// compared to the stored hash in the database to find the invitation. This is done to avoid storing the invitation codein
    /// plaintext in the database for security reasons. If an ownership invitation with the provided invitation codeis found,
    /// it is decrypted and returned. Otherwise, an error result is returned indicating that no ownership invitation
    /// was found with the provided invitationCode. This method is useful for accepting ownership invitations, where the invitationCode
    /// is provided in the invitation link and needs to be validated and used to retrieve the corresponding ownership
    /// invitation.
    /// </summary>
    /// <param name="invitationCode"></param>
    /// <param name="filters"></param>
    /// <param name="includeUsers"></param>
    /// <param name="includePet"></param>
    /// <returns></returns>
    public async Task<Result<OwnershipInvitation?>> GetOwnershipInvitationByCodeAsync(
        string invitationCode,
        StatusFilters? filters = null,
        bool includeUsers = true,
        bool includePet = true
    )
    {
        if (string.IsNullOrWhiteSpace(invitationCode))
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "INVALID_OWNERSHIP_INVITATION_NONCE",
                Status = 400,
                Message = "The provided ownership invitation invitation code is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // hash invitationCode
        var invitationCodeHashResult = SecurityService.HashWithSalt(invitationCode);
        if (!invitationCodeHashResult || invitationCodeHashResult.Data == null)
            return invitationCodeHashResult.Log(_logger).ConvertTo<OwnershipInvitation?>();

        // Find the ownership invitation
        var query = _dbContext.OwnershipInvitations
            .Where(p => p.InvitationCodeHash == invitationCodeHashResult.Data);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeUsers) query = query.Include(i => i.User);
        if (includePet) query = query.Include(i => i.Pet);

        // Execute query
        var invitation = await query.FirstOrDefaultAsync();
        if (invitation is null)
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "OWNERSHIP_INVITATION_NOT_FOUND",
                Status = 404,
                Message = "No ownership invitation found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return ownership invitation
        return new Result<OwnershipInvitation?>
        {
            Success = true,
            Code = "OWNERSHIP_INVITATION_FOUND",
            Status = 200,
            Message = "Ownership invitation found successfully",
            Data = invitation,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously deletes an ownership invitation by its ID. This method first retrieves the ownership invitation
    /// from the database using the provided ID. If the ownership invitation is found, it is removed from the database
    /// context and the changes are saved to the database. If the deletion is successful, a success result is returned.
    /// If the ownership invitation is not found or if an error occurs during deletion, an appropriate error result is
    /// returned indicating the reason for the failure. This method is useful for allowing users to delete ownership
    /// invitations that they have created or received, providing them with control over their shared pets and
    /// invitations.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result> DeleteOwnershipInvitationAsync(int id)
    {
        if (id <= 0)
        {
            return new Result
            {
                Success = false,
                Code = "INVALID_OWNERSHIP_INVITATION_ID",
                Status = 400,
                Message = "The provided ownership invitation ID is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };
        }

        // Get the invitation
        var existingInvitation = await _dbContext.OwnershipInvitations
            .FirstOrDefaultAsync(i => i.Id == id);
        if (existingInvitation is null)
            return new Result
            {
                Success = false,
                Code = "OWNERSHIP_INVITATION_NOT_FOUND",
                Status = 404,
                Message = "No ownership invitation found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Delete the invitation
        _dbContext.OwnershipInvitations.Remove(existingInvitation);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result
            {
                Success = false,
                Code = "ERROR_DELETING_OWNERSHIP_INVITATION",
                Status = 500,
                Message = "An error occurred while deleting the ownership invitation",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result
        {
            Success = true,
            Code = "OWNERSHIP_INVITATION_DELETED",
            Status = 200,
            Message = "Ownership invitation deleted successfully",
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    #endregion
}