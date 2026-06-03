using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

/// <remarks>FA02</remarks>
public class AppointmentsService(
    IAppointmentsRepository appointmentsRepo,
    IAppConfigService appConfigService,
    ILogger<PetService> logger
) : IAppointmentsService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A repo to handle appointments in the database
    /// </summary>
    private readonly IAppointmentsRepository _appointmentsRepo = appointmentsRepo;

    /// <summary>
    /// The service to get and set app configurations
    /// </summary>
    private readonly IAppConfigService _appConfigService = appConfigService;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<PetService> _logger = logger;

    /// <summary>
    /// A default number of minutes each appointment should last, in case the config is not set or is invalid.
    /// </summary>
    private const int DefaultAppointmentDurationInMinutes = 20;

    /// <summary>
    /// A default number of days to populate appointments for, in case the config is not set or is invalid. We will
    /// populate appointments for this number of days starting from now.
    /// </summary>
    private const int DefaultDaysToPopulateAppointments = 3;


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
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private async Task<Result<List<Appointment>>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.GetByTimeRangeAsync(start, end),
                operationName: "getting existing appointments by time range",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting existing appointments by time range");
            return new Result<List<Appointment>>
            {
                Success = false,
                Code = "ERROR_GETTING_EXISTING_APPOINTMENTS_BY_TIME_RANGE",
                Status = 500,
                Message = "An error occurred while getting existing appointments by time range",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result> PopulateAppointments()
    {
        try
        {
            // Get duration of appointments from config,
            // if not set then use the default value
            var appointmentDurationResult = _appConfigService.GetConfig(
                AppConfigKeys.AppointmentDurationInMinutes
            );
            var appointmentDurationInMinutes = int.TryParse(
                appointmentDurationResult?.Data?.ToString(),
                out var parsedAppointmentDurationInMinutes
            )
                ? parsedAppointmentDurationInMinutes
                : DefaultAppointmentDurationInMinutes;

            // This is the period of time we will populate,
            // meaning, this is the number of days we will
            // populate starting now
            var daysToPopulateAppointmentsResult = _appConfigService.GetConfig(
                AppConfigKeys.DaysToPopulateAppointments
            );
            var daysToPopulateAppointments = int.TryParse(
                daysToPopulateAppointmentsResult?.Data?.ToString(),
                out var parsedTimePeriodInDays
            )
                ? parsedTimePeriodInDays
                : DefaultDaysToPopulateAppointments;

            // Query DB to get appointments fron now until
            // daysToPopulateAppointments
            var start = DateTime.UtcNow;
            var end = DateTime.UtcNow.AddDays(daysToPopulateAppointments);
            var existingResult = await GetByTimeRangeAsync(start, end);





            //
            //
            //
            // var timePeriode = = _appConfigService.GetConfig(
            //     AppConfigKeys.AppointmentDurationInMinutes
            // );
            // Console.WriteLine(appointmentDurationInMinutes.Data);
            //
            // return new Result
            // {
            //     Success = false,
            //     Code = "BEBUGGING",
            //     Status = 200,
            //     Message = "Debbuging my bruh",
            //     Returnable = true
            // };

            return new Result
            {
                Success = true,
                Code = "APPOINTMENTS_POPULATED",
                Status = 200,
                Message = "Appointments populated successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error accepting pet ownership");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_ACCEPTING_PET_OWNERSHIP",
                Status = 500,
                Message = "An error occurred while accepting pet ownership",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}