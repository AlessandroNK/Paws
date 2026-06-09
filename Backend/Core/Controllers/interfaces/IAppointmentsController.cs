using Backend.Core.Models.Appointments;
using Backend.Core.Models.Results;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers.interfaces;

public interface IAppointmentsController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


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
    public Task<IActionResult> GetAvailableAppointmentsAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AppointmetDayRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    public Task<IActionResult> PopulateAppointments(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AppointmetDayRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    public Task<IActionResult> ReserveAppointmentAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] ReserveAppointmentRequest request
        );
}