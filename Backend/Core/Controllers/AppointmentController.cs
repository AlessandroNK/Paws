using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Results;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers;

/// <remarks>FA01</remarks>
[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowFrontend")]
public class AppointmentController(
    IAppointmentService appointmentService,
    ILogger<UserController> logger
) : ControllerBase, IAppointmentsController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<UserController> _logger = logger;

    /// <summary>
    /// The appointment service is used to manage the appointment operations. It is used to add, get, and populate
    /// appointments.
    /// </summary>
    private readonly IAppointmentService _appointmentService = appointmentService;


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
    [HttpGet]
    [Route("get-available-appointments")]
    public async Task<IActionResult> GetAvailableAppointmentsAsync(
        [FromHeader(Name = "Device-Id")] string deviceId
    )
    {
        try
        {
            _logger.LogInformation("Getting appointments for device {DeviceId}", deviceId);

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _appointmentService.GetAvailableAppointmentsAsync(includeVet: true);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.Map(
                    vets => vets.Select(a => a.ToDto()).ToList()
                ))
                : BadRequest(result.ToDto());
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
    [HttpPost]
    [Route("populate")]
    public async Task<IActionResult> PopulateAppointments(
        [FromHeader(Name = "Device-Id")] string deviceId
    )
    {
        try
        {
            _logger.LogInformation("Populating appointments");

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Sign the user up
            var result = await _appointmentService.PopulateAppointments();

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToDto())
                : BadRequest(result.ToDto());
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
}