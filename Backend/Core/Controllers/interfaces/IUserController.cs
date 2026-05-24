using Backend.Core.DTOs.Requests;
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
}