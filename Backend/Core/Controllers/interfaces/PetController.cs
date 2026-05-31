using Backend.Core.Models.Pets;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers.interfaces;

[ApiController]
[Microsoft.AspNetCore.Components.Route("api/[controller]")]
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
    [HttpPost]
    [Route("share-ownership")]
    public async Task<IActionResult> SharePetOwnership(
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
        var result = await _petService.SharePetOwnershipAsync(request);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
    }
}