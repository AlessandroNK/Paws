using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
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
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign up request</param>
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
            var result = await _userService.SignUp(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<BasicUserResponse>())
                : BadRequest(result.ToDto<BasicUserResponse>());
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Verifies the account by checking if the provided code matches the database's verification code for the specified
    /// user.
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The <see cref="AccountVerificationRequest"/> containing the email and the verification
    /// code</param>
    /// <returns></returns>
    [HttpPost]
    [Route("verify-account")]
    public async Task<IActionResult> VerifyAccountAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AccountVerificationRequest request
    )
    {
        try
        {
            _logger.LogInformation("Verifying account for user with device id: {DeviceId} and email: {@Email}",
                deviceId,
                request.Email);

            // Validations
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _userService.VerifyAccountAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<BasicUserResponse>())
                : BadRequest(result.ToDto<BasicUserResponse>());
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resends the verification code tot he user
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmailAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] ResendVerificationCodeRequest request
    )
    {
        try
        {
            _logger.LogInformation(
                "Resending verification email for user with device id: {DeviceId} and email: {@Email}",
                deviceId,
                request.Email);

            // Validations
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Resend the verification code
            var result = await _userService.ResendVerificationEmailAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<BasicUserResponse>())
                : BadRequest(result.ToDto<BasicUserResponse>());
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
    [HttpPost]
    [Route("add-new-pet")]
    public async Task<IActionResult> AddNewPetAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AddNewPetRequest request
    )
    {
        try
        {
            _logger.LogInformation("Adding new pet to user with device id: {DeviceId} and pet name: {@PetName}",
                deviceId,
                request.Pet.Name);

            // Validations
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the pet up
            var result = await _userService.AddNewPetAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<UserResponse>())
                : BadRequest(result.ToDto<UserResponse>());
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