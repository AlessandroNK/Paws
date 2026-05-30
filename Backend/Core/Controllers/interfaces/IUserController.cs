using Backend.Core.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers.interfaces;

/// <summary>
/// Defines how the user controller should look like. It is used to make sure that the user controller has all the
/// necessary methods and properties to work with the user model and the user service.
/// </summary>
public interface IUserController
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
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public Task<IActionResult> SignUp(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SignUpRequest request
        );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Verifies the account by checking if the provided code matches the database's verification code for the specified
    /// user.
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The <see cref="AccountVerificationRequest"/> containing the email and the verification
    /// code</param>
    /// <returns></returns>
    public Task<IActionResult> VerifyAccountAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AccountVerificationRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resends the verification code tot he user
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IActionResult> ResendVerificationEmailAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] ResendVerificationCodeRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IActionResult> AddNewPetAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AddNewPetRequest request
    );
}