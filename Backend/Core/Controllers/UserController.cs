using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Result;
using Backend.Core.Models.User;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers;

/// <summary>
///
/// </summary>
/// <param name="userService"></param>
/// <param name="logger"></param>
/// <remarks>FU03</remarks>
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

    #region HttpMethods

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

            // Validations
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _userService.SignUp(deviceId, request);
            return result ? Ok(result) : BadRequest(result);
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return Ok(new Result
            {
                Success = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Message = "Something is breaking inside the API",
                TraceCode = FileCodes.CallerIC()
            });
        }
    }

    #endregion
}