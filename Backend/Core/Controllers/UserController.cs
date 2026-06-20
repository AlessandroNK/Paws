using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
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
/// <remarks>FU01</remarks>
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
    /// We wanna log!!!
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
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _userService.SignUp(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<BasicUserResponse>())
                : BadRequest(result.ToApiResponse<BasicUserResponse>());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
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
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _userService.VerifyAccountAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<BasicUserResponse>())
                : BadRequest(result.ToApiResponse<BasicUserResponse>());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resends the verification code to the user
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
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Resend the verification code
            var result = await _userService.ResendVerificationEmailAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<OnlyEmailUserResponse>())
                : BadRequest(result.ToApiResponse<OnlyEmailUserResponse>());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a provided token to validate the user's session
    /// </summary>
    /// <param name="deviceId">The ID of the device sending the request</param>
    /// <param name="sessionToken">The session token to be validated</param>
    /// <returns></returns>
    [HttpGet]
    [Route("validate-session-token")]
    public async Task<IActionResult> ValidateSessionTokenAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "Session-Token")] string sessionToken
    )
    {
        try
        {
            _logger.LogInformation("Checking session token for user");

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Check the session token
            var result = await _userService.ValidateSessionTokenAsync(deviceId, sessionToken);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<BasicUserResponse>())
                : BadRequest(result.ToApiResponse<BasicUserResponse>());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Starts the login process by sending a verification code to the user's email. It takes the device id from the
    /// header and the login request from the body. It returns an IActionResult with some relevant data as ok, code, and
    /// status
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("start-login-process")]
    public async Task<IActionResult> StartLoginProcessAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] LoginRequest request
    )
    {
        try
        {
            _logger.LogInformation("Logging in user with device id: {DeviceId} and email: {@Email}", deviceId,
                request.Email);

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Log the user in
            var result = await _userService.StartLoginProcessAsync(deviceId, request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse())
                : BadRequest(result.ToApiResponse());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    [HttpPost]
    [Route("login-with-code")]
    public async Task<IActionResult> loginWithCode(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] LoginWithCodeRequest request
    )
    {
        try
        {
            _logger.LogInformation("Logging in user with device id: {DeviceId} and email: {@Email}", deviceId,
                request.Email);

            // Validations

            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Log the user in
            var result = await _userService.LoginWithCodeAsync(deviceId, request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<UserResponse>())
                : BadRequest(result.ToApiResponse<UserResponse>());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return BadRequest(new ApiResponse
                {
                    Success = false,
                    Kind = ApiResponseKind.Error,
                    Code = "BAD_OPERATION",
                    Status = 500,
                    Title = "Something is breaking inside the API",
                    TraceCode = FileCodes.CallerIC()
                }
            );
        }
    }

#endregion
}