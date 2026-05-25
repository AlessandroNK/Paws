using Backend.Core.Models.User;
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
    /// Signs in an existing user. It takes the device id from the header and the sign in request from the body. It
    /// returns an IActionResult with some relevant data as ok, code, an the user basic data (does not return sensitive
    /// data such the password
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign in request</param>
    /// <returns></returns>
    public Task<IActionResult> SignIn(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SignInRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs out an existing user. It takes the device id and the authorization token from the header and the sign out
    /// request from the body. It returns an IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="authorization">The authorization token of the user</param>
    /// <param name="request">The sign out request</param>
    /// <returns></returns>
    public Task<IActionResult> SignOut(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "Authorization")] string authorization,
        [FromBody] SignOutRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the status of an existing user. It takes the device id and the authorization token from the header. It
    /// returns an IActionResult with some relevant data as ok, code, status, and the user basic data (does not return
    /// sensitive data such the password)
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="authorization">The authorization token of the user</param>
    /// <returns></returns>
    public Task<IActionResult> GetUserStatus(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "Authorization")] string authorization
    );
}