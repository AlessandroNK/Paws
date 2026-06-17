using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Models.Vets;
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
public class VetController(
    IVetService vetService,
    ILogger<VetController> logger
) : ControllerBase, IVetController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<VetController> _logger = logger;

    /// <summary>
    /// The vet service is used to manage the vet operations. It is used to sign up, sign in, sign out, and get the
    /// status of the vet.
    /// </summary>
    private readonly IVetService _vetService = vetService;

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
    /// Gets all vets in the system
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <returns></returns>
    [HttpGet]
    [Route("get-vets")]
    public async Task<IActionResult> GetVetsAsync(
        [FromHeader(Name = "Device-Id")] string deviceId
    )
    {
        try
        {
            _logger.LogInformation("Getting all vets");

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _vetService.GetVetsAsync();

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.Map(
                    vets => vets.Select(v => v.ToDto()).ToList()
                ))
                : BadRequest(result.ToApiResponse());
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e);
            return Ok(new Result
            {
                Success = false,
                Code = "BAD_OPERATION",
                Status = 500,
                Title = "Something is breaking inside the API",
                TraceCode = FileCodes.CallerIC()
            });
        }
    }

    #endregion
}