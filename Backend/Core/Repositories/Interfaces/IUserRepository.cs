using Backend.Core.DTOs.Requests;
using Backend.Core.Models;

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
    /// <param name="email">The email of the user</param>
    /// <param name="excludeHidden">Whether to filter out hidden users</param>
    /// <returns>The user if any</returns>
    public Task<Result<User?>> GetByEmailAsync(string email, bool excludeHidden);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document of the user</param>
    /// <param name="excludeHidden">Whether to filter out hidden users</param>
    /// <returns>The user if any</returns>
    public Task<Result<User?>> GetByDocumentAsync(string document, bool excludeHidden);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the sign up was successful</returns>
    public Task<Result> AddAsync(User user);
}