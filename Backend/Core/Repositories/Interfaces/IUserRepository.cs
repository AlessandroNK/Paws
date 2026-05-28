using Backend.Core.Models.Result;
using Backend.Core.Models.User;

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
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByEmailAsync(
        string email,
        bool excludeInactive = true,
        bool excludeBanned = true
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <returns>The user if any</returns>
    public Task<Result<User?>> GetByDocumentAsync(
        string document,
        bool excludeInactive = true,
        bool excludeBanned = true
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the sign up was successful</returns>
    public Task<Result<User?>> AddAsync(User user);
}