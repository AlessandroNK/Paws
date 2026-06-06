using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers;

/// <remarks>FP01</remarks>
[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowFrontend")]
public class PetController(
    IPetService petService,
    ILogger<UserController> logger
) : ControllerBase, IPetController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A service to handle pet related operations. It is used to manage the pet operations, such as sharing pet ownership.
    /// </summary>
    private readonly IPetService _petService = petService;

    /// <summary>
    /// We wanna log!!!
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

    #region HttpMethods

    /// <summary>
    /// Shares the ownership of a pet with another user by sending an ownership invitation.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="invitationRequest">The <see cref="SendOwnershipInvitationRequest"/> request</param>
    /// <returns></returns>
    [HttpPost]
    [Route("share-ownership")]
    public async Task<IActionResult> SendOwnershipInvitationAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SendOwnershipInvitationRequest invitationRequest
    )
    {
        _logger.LogInformation(
            "Received request to send ownership invitation for pet {@PetId} to user {@UserId} and device ID: {DeviceId}",
            invitationRequest.PetId, invitationRequest.UserId, deviceId
        );

        // Validations
        Env.SetInteractionCode(deviceId);
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
        if (!deviceValidationResult) return BadRequest(deviceValidationResult);

        // Send the ownership invitation
        var result = await _petService.SendOwnershipInvitationAsync(invitationRequest);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Accepts an ownership invitation for a pet.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="invitationCode"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("accept-ownership")]
    public async Task<IActionResult> AcceptOwnershipInvitation(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AcceptOwnershipInvitationRequest invitationCode
    )
    {
        _logger.LogInformation(
            "Received request to accept ownership invitation with code {@InvitationCode} and device ID: {DeviceId}",
            invitationCode.InvitationCode, deviceId
        );

        // Validations
        Env.SetInteractionCode(deviceId);
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
        if (!deviceValidationResult) return BadRequest(deviceValidationResult);

        // Accept the ownership invitation
        var result = await _petService.AcceptOwnershipInvitationAsync(invitationCode);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
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
            Env.SetInteractionCode(deviceId);
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the pet up
            var result = await _petService.AddNewPetAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<PetResponse>())
                : BadRequest(result.ToDto<PetResponse>());
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
    /// Removes a pet from the user. It takes the device id from the header and the remove pet from user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the removed pet data. It also
    /// checks if the user is verified before removing the pet from the user. If the user is not verified, it returns a
    /// bad request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("remove-pet")]
    public async Task<IActionResult> RemovePetAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] RemovePetRequest request
    )
    {
        try
        {
            _logger.LogInformation("Removing pet from user with device id: {DeviceId} and pet id: {@PetId}",
                deviceId,
                request.PetId);

            // Validations
            Env.SetInteractionCode(deviceId);
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Remove the pet
            var result = await _petService.RemovePetAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto<PetResponse>())
                : BadRequest(result.ToDto<PetResponse>());
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