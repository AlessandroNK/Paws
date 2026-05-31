using Backend.Core.Models.Pets;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers.interfaces;

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
    /// Shares the ownership of a pet with another user. It sends an invitation to the user to accept the ownership of
    /// the pet.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="request">The <see cref="SharePetOwnershipRequest"/> request</param>
    /// <returns></returns>
    [HttpPost]
    [Route("share-ownership")]
    public async Task<IActionResult> SendOwnershipInvitationAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SharePetOwnershipRequest request
    )
    {
        _logger.LogInformation(
            "Received request to share pet ownership: {@PetId} with user {@UserId} and device ID: {DeviceId}",
            request.PetId, request.UserId, deviceId
        );

        // Validations
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
        if (!deviceValidationResult) return BadRequest(deviceValidationResult);

        // Share the ownership
        var result = await _petService.SendOwnershipInvitationAsync(request);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
    }

    #endregion
}