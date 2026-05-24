using Backend.Core.Controllers.interfaces;
using Backend.Core.DTOs.Requests;
using Backend.Core.Internal;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowFrontend")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger
    ) : ControllerBase, IUserController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// We wanna do logs!!!
    /// </summary>
    private readonly ILogger<UserController> _logger = logger;

    /// <summary>
    /// The user service is used to manage the user operations. It is used to sign up, sign in, sign out, and get the
    /// status of the user.
    /// </summary>
    private readonly IUserService _userService = userService;


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
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("sign-up")]
    public async Task<IActionResult> SignUp(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SignUpRequest request
    )
    {
        try
        {
            _logger.LogInformation("Signing up user with device id: {DeviceId} and email: {@Email}", deviceId,
                request.Email);


            var result = _userService.SignUp(deviceId, request);
            return Ok(new
            {

            });
        }
        catch (Exception e)
        {
            Helpers.LogError(_logger, e);
            return Ok(new
            {
                Ok = false,
                IsSuccess = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Message = "Something is breaking inside the API"
            });
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs in an existing user. It takes the device id from the header and the sign in request from the body. It
    /// returns an IActionResult with some relevant data as ok, code, an the user basic data (does not return sensitive
    /// data such the password
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign in request</param>
    /// <returns></returns>
    [HttpPost]
    [Route("sign-in")]
    public async Task<IActionResult> SignIn(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SignInRequest request
    )
    {
        try
        {
            _logger.LogInformation("Signing up user with device id: {DeviceId} and email: {@Email}", deviceId,
                request.Email);


            return Ok(new
            {
                Ok = true,
                IsSuccess = false,
                Code = "USER_SIGNED_IN",
                Status = 200,
                Message = "User signed in successfully"
            });
        }
        catch (Exception e)
        {
            Helpers.LogError(_logger, e);
            return Ok(new
            {
                Ok = false,
                IsSuccess = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Message = "Something is breaking inside the API"
            });
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs out an existing user. It takes the device id and the authorization token from the header and the sign out
    /// request from the body. It returns an IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="authorization">The authorization token of the user</param>
    /// <param name="request">The sign out request</param>
    /// <returns></returns>
    [HttpPost]
    [Route("sign-out")]
    public async Task<IActionResult> SignOut(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "Authorization")] string authorization,
        [FromBody] SignOutRequest request
    )
    {
        try
        {
            _logger.LogInformation("Signing out user with device id: {DeviceId} and id: {@Id}", deviceId,
                request.UserId);


            return Ok(new
            {
                Ok = true,
                IsSuccess = false,
                Code = "USER_SIGNED_OUT",
                Status = 200,
                Message = "User signed out successfully"
            });
        }
        catch (Exception e)
        {
            Helpers.LogError(_logger, e);
            return Ok(new
            {
                Ok = false,
                IsSuccess = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Message = "Something is breaking inside the API"
            });
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the status of an existing user. It takes the device id and the authorization token from the header. It
    /// returns an IActionResult with some relevant data as ok, code, status, and the user basic data (does not return
    /// sensitive data such the password)
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="authorization">The authorization token of the user</param>
    /// <returns></returns>
    [HttpGet]
    [Route("status")]
    public async Task<IActionResult> GetUserStatus(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "Authorization")] string authorization
    )
    {
        try
        {
            _logger.LogInformation("Getting user status with device id: {DeviceId} and authorization: {@Authorization}",
                deviceId,
                authorization);

            return Ok(new
            {
                Ok = true,
                IsSuccess = false,
                Code = "USER_LOGGED_IN",
                Status = 200,
                Message = "User is logged in, session restored successfully",
                Data = new
                {
                    Id = 1,
                    Email = "user@example.com"
                }
            });
        }
        catch (Exception e)
        {
            Helpers.LogError(_logger, e);
            return Ok(new
            {
                Ok = false,
                IsSuccess = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Message = "Something is breaking inside the API"
            });
        }
    }
}