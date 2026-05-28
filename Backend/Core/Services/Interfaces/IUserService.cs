using Backend.Core.Models.Result;
using Backend.Core.Models.User;

namespace Backend.Core.Services.Interfaces;

public interface IUserService
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
    /// <param name="document">The document to search for</param>
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <returns>The created user</returns>
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
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public Task<Result<User?>> SignUp(string deviceId, SignUpRequest request);
}