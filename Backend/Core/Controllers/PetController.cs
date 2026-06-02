using Backend.Core.Controllers.interfaces;
using Backend.Core.Models.Pets;
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
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
        if (!deviceValidationResult) return BadRequest(deviceValidationResult);

        // Accept the ownership invitation
        var result = await _petService.AcceptOwnershipInvitationAsync(invitationCode);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
    }

    #endregion
}