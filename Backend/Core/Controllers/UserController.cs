using Backend.Core.Controllers.interfaces;
using Backend.Core.DTOs.Requests;
using Backend.Core.Internal;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[EnableCors("AllowFrontend")]
public class UserController(ILogger<UserController> logger) : ControllerBase, IUserController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// We wanna do logs!!!
    /// </summary>
    private readonly ILogger<UserController> _logger = logger;


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
    [Route("/sign-up")]
    public async Task<IActionResult> SignUp(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SignUpRequest request
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
                Code = "USER_SIGNED_UP",
                Status = 200,
                Message = "User signed up successfully"
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
    [Route("/sign-in")]
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
                Code = "USER_SIGNED_UP",
                Status = 200,
                Message = "User signed up successfully"
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