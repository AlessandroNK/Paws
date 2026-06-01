using Backend.Core.Data;
using Backend.Core.Encryption;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

/// <summary>
/// Provides functionality to access and manage users in the application.
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
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<UserRepository> _logger = logger;


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
    private static IQueryable<EncryptedUser> ApplyStatusFilters(
        IQueryable<EncryptedUser> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(i => i.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(i => i.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(i => i.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(i => i.Status != EntityStatus.Banned);
        if (!filters.IncludeUnverified) query = query.Where(i => i.Status != EntityStatus.Unverified);

        return query;
    }

    // -----------------------------------------------------------------------------------------------------------------
    private static IQueryable<EncryptedUserPet> ApplyStatusFilters(
        IQueryable<EncryptedUserPet> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(i => i.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(i => i.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(i => i.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(i => i.Status != EntityStatus.Banned);
        if (!filters.IncludeUnverified) query = query.Where(i => i.Status != EntityStatus.Unverified);

        return query;
    }




    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------

    #region DB

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null
    )
    {
        // Encrypt user data to find it in the db
        var hashedEmailResult = SecurityService.HashWithSalt(email);
        if (!hashedEmailResult || hashedEmailResult.Data == null)
            return hashedEmailResult.Log(_logger).ConvertTo<User?>();

        // Find the user
        var query = _dbContext.EncryptedUsers
            .Where(u => u.EmailHash == hashedEmailResult.Data);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .Include(u => u.UserPets)
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
        return UserEncryption.DecryptUser(encryptedUser, _logger);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The user if any</returns>
    public async Task<Result<User?>> GetByDocumentAsync(
        string document,
        StatusFilters? filters = null
    )
    {
        // Encrypt user data to find it in the db
        var hashedDocumentResult = SecurityService.HashWithSalt(document);
        if (!hashedDocumentResult || hashedDocumentResult.Data == null)
        {
            _logger.LogError("Failed to encrypt document for user lookup: {ErrorMessage}",
                hashedDocumentResult.Message);
            return hashedDocumentResult.ConvertTo<User?>();
        }

        // Find the user
        var query = _dbContext.EncryptedUsers.AsQueryable()
            .Where(u => u.DocumentHash == hashedDocumentResult.Data);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .Include(u => u.UserPets)
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
        return UserEncryption.DecryptUser(encryptedUser, _logger);
    }

    /// <sumary>
    /// Finds a user by its ID.
    /// </sumary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>A <see cref="Result{User}"/> indicating the result of the operation and including the user if it was found</returns>
    public async Task<Result<User?>> GetByIdAsync(int id, StatusFilters? filters = null)
    {
        if (id <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "INVALID_USER_ID",
                Status = 400,
                Message = "Invalid user ID provided",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the user
        var query = _dbContext.EncryptedUsers
            .Where(u => u.Id == id);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .Include(u => u.UserPets)
            .ThenInclude(p => p.EncryptedPet)
            .AsSplitQuery();

        // Execute query
        var encryptedUser = await query.FirstOrDefaultAsync();
        if (encryptedUser is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Message = "No user found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return user
        return UserEncryption.DecryptUser(encryptedUser, _logger);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It returns an <see cref="Result{User}"/> indicating the result of the operation and including
    /// the user if it was successful.
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the sign up was successful</returns>
    public async Task<Result<User?>> AddAsync(User user)
    {
        // Encrypt user data
        var result = UserEncryption.EncryptUser(user, _logger);
        if (!result || result.Data is null) return result.ConvertTo<User?>();

        // Save the user
        _dbContext.EncryptedUsers.Add(result.Data);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
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
        var filters = StatusFilters.IncludeAll();
        var getUserResult = await GetByEmailAsync(user.Email, filters);
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
    /// an instance of the updated user if the update was successful, or an error result if something went wrong.
    /// The filters parameter can be used to specify which user statuses should be included in the query when looking for
    /// the user to update.
    /// </summary>
    /// <param name="user">the <see cref="User"/> to update</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The <see cref="User"/></returns>
    public async Task<Result<User?>> UpdateAsync(User user, StatusFilters? filters = null)
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
        var updateResult = UserEncryption.EncryptUserAndUpdateTrackedEntity(user, existingEncryptedUser, _logger);
        if (!updateResult) return updateResult.ConvertTo<User?>();

        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_UPDATING_USER",
                Status = 500,
                Message = "An error occurred while updating the user",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        var getUserResult = await GetByEmailAsync(user.Email, filters);
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds a pet to a user. It takes a <see cref="UserPet"/> relationship and creates the corresponding
    /// <see cref="EncryptedUserPet"/> in the database. It returns the updated user with the new pet included.
    /// </summary>
    /// <param name="userPet"></param>
    /// <returns></returns>
    public async Task<Result<User?>> AddUserPet(UserPet userPet)
    {
        // Validations
        if (userPet.UserId <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_ID_NOT_PROVIDED",
                Status = 400,
                Message = "User id not provided for user-pet relationship",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        if (userPet.PetId <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "PET_ID_NOT_PROVIDED",
                Status = 400,
                Message = "Pet id not provided for user-pet relationship",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Encrypt the relationship (this is necessary only
        // because we need a dto where we can get the encrypted
        // user and pet when retrieving them from the DB)
        var encrypted = new EncryptedUserPet
        {
            EncryptedUserId = userPet.UserId,
            EncryptedPetId = userPet.PetId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = EntityStatus.Active
        };

        // Save the relationship
        _dbContext.EncryptedUserPets.Add(encrypted);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_CREATING_USER_PET_RELATIONSHIP",
                Status = 500,
                Message = "An error occurred while creating the user-pet relationship",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        var filters = StatusFilters.IncludeAll();
        var userResult = await GetByIdAsync(userPet.UserId, filters);
        if (!userResult || userResult.Data == null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_PET_RELATIONSHIP_CREATED_BUT_USER_NOT_FOUND",
                Status = 500,
                Message = "User-pet relationship created but user not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<User?>
        {
            Success = true,
            Code = "USER_PET_RELATIONSHIP_CREATED",
            Status = 201,
            Message = "User-pet relationship created successfully",
            Data = userResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    #endregion

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user-pet relationship by the user id and pet id. It returns the relationship if found, or an error result if not.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="petId"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public async Task<Result<UserPet?>> GetUserPetByBothIdsAsync(int userId, int petId, StatusFilters? filters = null)
    {
        if (userId <= 0)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "USER_ID_NOT_PROVIDED",
                Status = 400,
                Message = "User id not provided for user-pet relationship lookup",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        if (petId <= 0)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "PET_ID_NOT_PROVIDED",
                Status = 400,
                Message = "Pet id not provided for user-pet relationship lookup",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the user pet
        var query = _dbContext.EncryptedUserPets
            .Where(up => up.EncryptedUserId == userId && up.EncryptedPetId == petId);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .Include(p => p.EncryptedUser)
            .Include(p => p.EncryptedPet)
            .AsSplitQuery();

        // Execute query
        var encryptedUserPet = await query.FirstOrDefaultAsync();
        if (encryptedUserPet is null)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "PET_NOT_FOUND",
                Status = 404,
                Message = "No pet found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return
        return UserPetsEncryption.DecryptUserPet(encryptedUserPet, _logger).ConvertTo<UserPet?>();
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Removes a pet from the user. It takes the device id from the header and the remove pet from user request from
    /// the body. It checks if the user is verified before removing the pet from the user. If the user is not verified,
    /// it returns a bad request with a message indicating that the user is not verified. It also checks if the user is
    /// the owner of the pet before allowing them to remove the pet from their account. If the user is not the owner of
    /// the pet, it returns a bad request with a message indicating that the user is not the owner of the pet. Finally,
    /// it doesn't delete the relationship between the user and the pet, but instead it sets its status to deleted, so
    /// the information is not lost and can be used for analytics and other purposes in the future.
    /// </summary>
    /// <param name="userPet"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public async Task<Result<UserPet?>> UpdateUserPet(UserPet userPet, StatusFilters? filters = null)
    {
        if (userPet.Id <= 0)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "USER_PET_ID_NOT_PROVIDED",
                Status = 400,
                Message = "User pet id not provided for update",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // First get the existing encrypted user from the database
        var existingEncryptedUserPet = await _dbContext.EncryptedUserPets
            .FirstOrDefaultAsync(eu => eu.Id == userPet.Id);

        if (existingEncryptedUserPet == null)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "USER_PET_NOT_FOUND",
                Status = 404,
                Message = "User pet not found",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Update the tracked entity with new encrypted values
        existingEncryptedUserPet.EncryptedUserId = userPet.UserId;
        existingEncryptedUserPet.EncryptedPetId = userPet.PetId;
        existingEncryptedUserPet.UpdatedAt = DateTime.UtcNow;
        existingEncryptedUserPet.Status = userPet.Status;

        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "ERROR_UPDATING_USER_PET_RELATIONSHIP",
                Status = 500,
                Message = "An error occurred while updating the user-pet relationship",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        var getResult = await GetUserPetByBothIdsAsync(userPet.UserId, userPet.PetId, filters);
        if (!getResult || getResult.Data == null)
            return new Result<UserPet?>
            {
                Success = false,
                Code = "USER_PET_RELATIONSHIP_UPDATED_BUT_NOT_FOUND",
                Status = 500,
                Message = "User-pet relationship updated but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<UserPet?>
        {
            Success = true,
            Code = "USER_PET_RELATIONSHIP_UPDATED",
            Status = 200,
            Message = "User-pet relationship updated successfully",
            Data = getResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    #region Helpers

    #endregion
}