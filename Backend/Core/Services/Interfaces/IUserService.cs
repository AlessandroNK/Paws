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
    /// <returns>The created user</returns>
    public Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null
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
        StatusFilters? filters = null
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
    /// <summary>
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<Result<User?>> AddNewPetAsync(AddNewPetRequest request);

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
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<Result<User?>> RemovePetAsync(RemovePetRequest request);
}