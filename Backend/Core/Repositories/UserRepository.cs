using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

/// <summary>
/// Provides functionality to access and manage users in the application.
/// This interface is responsible for managing users in the application.
/// It provides methods for creating, updating, deleting, and retrieving users.
/// The implementation of this interface should handle the data access logic, such as interacting with a database
/// or any other data source.
/// </summary>
/// <remarks>FU03</remarks>
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
    private static IQueryable<User> ApplyStatusFilters(
        IQueryable<User> query,
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
    private static IQueryable<SessionToken> ApplyStatusFilters(
        IQueryable<SessionToken> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(t => t.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(t => t.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(t => t.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(t => t.Status != EntityStatus.Banned);
        if (!filters.IncludeUnverified) query = query.Where(t => t.Status != EntityStatus.Unverified);

        return query;
    }

    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false
    )
    {
        // Encrypt user data to find it in the db
        var hashedEmailResult = SecurityService.HashWithSalt(email);
        if (!hashedEmailResult || hashedEmailResult.Data == null)
            return hashedEmailResult.Log(_logger).ConvertTo<User?>();

        // Find the user
        var query = _dbContext.Users
            .Where(u => u.EmailHash == hashedEmailResult.Data);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includePets)
            query = query
                .Include(p => p.UserPets)
                .ThenInclude(up => up.Pet);

        if (includeSessionToken)
            query = query
                .Include(u => u.SessionToken);

        query = query.AsSplitQuery();

        // Execute query
        var user = await query.FirstOrDefaultAsync();
        if (user is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Title = "No user found with the provided email",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return user
        return new Result<User?>
        {
            Success = true,
            Code = "USER_FOUND",
            Status = 200,
            Title = "User found successfully",
            Data = user,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The user if any</returns>
    public async Task<Result<User?>> GetByDocumentAsync(
        string document,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false
    )
    {
        // Encrypt user data to find it in the db
        var hashedDocumentResult = SecurityService.HashWithSalt(document);
        if (!hashedDocumentResult || hashedDocumentResult.Data == null)
        {
            _logger.LogError("Failed to encrypt document for user lookup: {ErrorMessage}",
                hashedDocumentResult.Title);
            return hashedDocumentResult.ConvertTo<User?>();
        }

        // Find the user
        var query = _dbContext.Users.AsQueryable()
            .Where(u => u.DocumentHash == hashedDocumentResult.Data);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includePets)
            query = query
                .Include(p => p.UserPets)
                .ThenInclude(up => up.Pet);

        if (includeSessionToken)
            query = query
                .Include(u => u.SessionToken);

        query = query.AsSplitQuery();

        // Execute query
        var user = await query.FirstOrDefaultAsync();
        if (user is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Title = "No user found with the provided document",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Return user
        return new Result<User?>
        {
            Success = true,
            Code = "USER_FOUND",
            Status = 200,
            Title = "User found successfully",
            Data = user,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    /// <sumary>
    /// Finds a user by its ID.
    /// </sumary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>A <see cref="Result{User}"/> indicating the result of the operation and including the user if it was found</returns>
    public async Task<Result<User?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false
    )
    {
        if (id <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "INVALID_USER_ID",
                Status = 400,
                Title = "Invalid user ID provided",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the user
        var query = _dbContext.Users
            .Where(u => u.Id == id);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includePets)
            query = query
                .Include(p => p.UserPets)
                .ThenInclude(up => up.Pet);

        if (includeSessionToken)
            query = query
                .Include(u => u.SessionToken);

        query = query.AsSplitQuery();

        // Execute query
        var user = await query.FirstOrDefaultAsync();
        if (user is null)
            return new Result<User?>
            {
                Success = false,
                Code = "USER_NOT_FOUND",
                Status = 404,
                Title = "No user found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return user
        return new Result<User?>
        {
            Success = true,
            Code = "USER_FOUND",
            Status = 200,
            Title = "User found successfully",
            Data = user,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
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
        // Save the user
        _dbContext.Users.Add(user);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_CREATING_USER",
                Status = 500,
                Title = "An error occurred while creating the user",
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
                Title = "User created but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        user = getUserResult.Data;
        return new Result<User?>
        {
            Success = true,
            Code = "USER_CREATED",
            Status = 201,
            Title = "User created successfully",
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
        // Update the entity
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_UPDATING_USER",
                Status = 500,
                Title = "An error occurred while updating the user",
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
                Title = "User updated but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<User?>
        {
            Success = true,
            Code = "USER_UPDATED",
            Status = 200,
            Title = "User updated successfully",
            Data = getUserResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<SessionToken?>> GetSessionTokenByTokenAsync(
        string requestTokenHash,
        StatusFilters? filters = null,
        bool includeUser = false
    )
    {
        // Find this token
        var query = _dbContext.SessionTokens
            .Where(st => st.TokenHash == requestTokenHash);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeUser) query = query.Include(p => p.User);

        query = query.AsSplitQuery();

        // Execute query
        var sessionToken = await query.FirstOrDefaultAsync();
        if (sessionToken is null)
            return new Result<SessionToken?>
            {
                Success = false,
                Code = "SESSION_TOKEN_NOT_FOUND",
                Status = 404,
                Title = "No session token found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Decrypt and return
        return new Result<SessionToken?>
        {
            Success = true,
            Code = "SESSION_TOKEN_FOUND",
            Status = 200,
            Title = "Session token found successfully",
            Data = sessionToken,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    #region Helpers

    #endregion
}