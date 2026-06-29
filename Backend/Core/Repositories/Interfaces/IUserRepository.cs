using Backend.Core.Models.Intern;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;

namespace Backend.Core.Repositories.Interfaces;

/// <summary>
/// This interface defines how a users repository should be structured
/// </summary>
public interface IUserRepository
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
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The user if any</returns>
    public Task<Result<User?>> GetByDocumentAsync(
        string document,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <sumary>
    /// Finds a user by its ID.
    /// </sumary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePets">Whether to include the user's pets in the result</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <param name="includeAppointments">Whether to include the user's appointments in the result</param>
    /// <returns>A <see cref="Result{User}"/> indicating the result of the operation and including the user if it was found</returns>
    public Task<Result<User?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includePets = false,
        bool includeSessionToken = false,
        bool includeAppointments = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It returns an <see cref="Result{User}"/> indicating the result of the operation and including
    /// the user if it was successful.
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the sign up was successful</returns>
    public Task<Result<User?>> AddAsync(User user);

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
    public Task<Result<User?>> UpdateAsync(User user, StatusFilters? filters = null);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<SessionToken?>> GetSessionTokenByTokenAsync(
        string requestTokenHash,
        StatusFilters? filters = null,
        bool includeUser = false
    );
}