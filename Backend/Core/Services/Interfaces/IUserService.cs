using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;

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
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null,
        bool includeSessionToken = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByDocumentAsync(
        string document,
        StatusFilters? filters = null
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByIdAsync(
       int id,
        StatusFilters? filters = null,
        bool includePets = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public Task<Result<User?>> SignUp(SignUpRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Verifies the account by checking if the provided code matches the database's verification code for the specified
    /// user.
    /// </summary>
    /// <param name="request">The <see cref="AccountVerificationRequest"/> containing the email and the verification
    /// code</param>
    /// <returns></returns>
    public Task<Result<User?>> VerifyAccountAsync(AccountVerificationRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resends the verification code to the user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<Result<User?>> ResendVerificationEmailAsync(ResendVerificationCodeRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<User?>> ValidateSessionTokenAsync(string deviceId, string requestTokenHash);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Starts the login process by sending a verification code to the user's email. It takes the device id from the
    /// header and the login request from the body. It returns an IActionResult with some relevant data as ok, code, and
    /// status
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<Result<string>> StartLoginProcessAsync(string deviceId, LoginRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<User?>> LoginWithCodeAsync(string deviceId, LoginWithCodeRequest request);
}