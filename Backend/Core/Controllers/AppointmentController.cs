using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
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
    [HttpPost]
    [Route("get-available-appointments")]
    public async Task<IActionResult> GetAvailableAppointmentsAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AppointmetDayRequest request
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
            var result = await _appointmentService.GetAvailableAppointmentsAsync(request, includeVet: true);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.Map(vets => vets.Select(a => a.ToScheduleDto()).ToList()
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Populates the schedule of all vets for a specific day. This method is intended to be called by a scheduled job
    /// that runs every day at a specific time to populate the schedule of the vets for the next few days, so there are
    /// always available appointments for the users to book. The number of days to populate can be configured in the app
    /// settings, and if not set, it will default to a predefined value. The duration of each appointment can also be
    /// configured in the app settings, and if not set, it will default to a predefined value. The method will check for
    /// existing appointments before adding new ones to avoid duplicates, and it will only add appointments within the
    /// active hours of the vets.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("populate")]
    public async Task<IActionResult> PopulateAppointments(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AppointmetDayRequest request
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
            var result = await _appointmentService.PopulateAppointments(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse())
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Reserves an appointment for a user pet. This method will perform all the necessary validations to ensure that the
    /// appointment can be reserved, such as checking if the appointment is still available, if the user pet relationship
    /// exists and is valid, if the pet is active, and if the user is active. If all validations pass, the method will
    /// update the appointment with the user pet ID and change its status to scheduled. The method will return the updated
    /// appointment if the reservation is successful, or an error result if any.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("reserve-appointment")]
    public async Task<IActionResult> ReserveAppointmentAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] ReserveAppointmentRequest request
    )
    {
        try
        {
            _logger.LogInformation("Reserving appointment for device {DeviceId}", deviceId);

            // Validations
            Env.SetInteractionCode(deviceId);
            var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
            if (!deviceValidationResult) return BadRequest(deviceValidationResult);

            // Reserve the appointment
            var result = await _appointmentService.ReserveAppointmentAsync(request);

            // Clean the response and convert it and its data to Dto
            return result
                ? Ok(result.ToApiResponse<AppointmentResponse>())
                : BadRequest(result.ToApiResponse<AppointmentResponse>());
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
}