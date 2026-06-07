using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Models.Vets;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

/// <remarks>FA02</remarks>
public class AppointmentService(
    IAppointmentsRepository appointmentsRepo,
    IAppConfigService appConfigService,
    IVetRepository vetRepo,
    ILogger<PetService> logger
) : IAppointmentService
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
    /// Provides functionality to access and manage vets.
    /// </summary>
    private readonly IVetRepository _vetRepo = vetRepo;

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
    private async Task<Result<int>> PopulateDayForVet(
        Vet vet,
        DateTime day,
        int appointmentDuration
    )
    {
        try
        {
            // Validations
            if (day.DayOfWeek == DayOfWeek.Sunday)
                return new Result<int>
                {
                    Success = false,
                    Code = "CANNOT_POPULATE_SCHEDULE_FOR_VET_ON_SUNDAY",
                    Status = 400,
                    Message =
                        $"Cannot populate schedule for vet {vet.Id} on Sunday as the vet does not work on Sundays (yet).",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // These Vets work from 8am to 9pm with no break,
            // because we are abusive bosses with no consideration
            // for the wellbeing of the employees, and no pinch
            // of humanity
            var activeHours = new HourRange
            {
                Start = new TimeOnly(8, 0),
                End = new TimeOnly(21, 0),
            };

            // First, we start the appointment date
            // to the exact hour the vet opens so
            // no time wasted xD
            var appointmentDate = new DateTime(
                day.Year,
                day.Month,
                day.Day,
                activeHours.Start.Hour,
                activeHours.Start.Minute,
                0,
                DateTimeKind.Utc
            );

            // Start populating
            var totalNewAppointments = 0;
            var errors = 0;
            while (TimeOnly.FromDateTime(appointmentDate) < activeHours.End)
            {
                // Check for appointments inside active hours
                // because, even when we are so abusive, we
                // don't want to be sued xD
                var endTime = appointmentDate.AddMinutes(appointmentDuration);
                if (
                    TimeOnly.FromDateTime(appointmentDate) < activeHours.Start ||
                    TimeOnly.FromDateTime(endTime) > activeHours.End
                ) break;

                // If it fits, then add it
                var appointment = new Appointment
                {
                    EndTime = endTime,
                    StartTime = appointmentDate,
                    Status = AppointmentStatus.Available,
                    UserPetId = null,
                    VetId = vet.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save the appointment
                var result = await AddAsync(appointment);
                if (result) totalNewAppointments++;
                else errors++;
                appointmentDate = appointmentDate.AddMinutes(appointmentDuration);
            }

            // Finished!!
            return errors == 0
                ? new Result<int>
                {
                    Success = true,
                    Code = "SCHEDULE_POPULATED_SUCCESSFULLY_FOR_VET",
                    Status = 200,
                    Message =
                        $"Schedule populated successfully for vet {vet.Id}. Total appointments added: {totalNewAppointments}",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = totalNewAppointments
                }
                : new Result<int>
                {
                    Success = false,
                    Code = "SCHEDULE_POPULATED_WITH_ERRORS_FOR_VET",
                    Status = 500,
                    Message =
                        $"Schedule populated for vet {vet.Id} with {errors} errors. Total appointments added: {totalNewAppointments}. Check logs for more details.",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = totalNewAppointments
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error populating schedule for vet {vet.Id}");
            return new Result<int>
            {
                Success = false,
                Code = "ERROR_POPULATING_SCHEDULE_FOR_VET",
                Status = 500,
                Message =
                    $"An error occurred while populating the schedule for vet {vet.Id}. Check logs for more details.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<Appointment?>> AddAsync(Appointment appointment)
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.AddAsync(appointment),
                operationName: "adding new appointment",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding new appointment");
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_ADDING_NEW_APPOINTMENT",
                Status = 500,
                Message = "An error occurred while adding a new appointment",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="timeRange">The time range to search for appointments</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePartialAppointments">Whether to include appointments that partially overlap with the specified
    /// time range. If false, only appointments that are completely within the time range will be included.</param>
    /// <returns></returns>
    public async Task<Result<List<Appointment>>> GetByTimeRangeAsync(
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false
    )
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.GetByTimeRangeAsync(timeRange, filters, includePartialAppointments),
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="vetId">The vet ID to search for appointments</param>
    /// <param name="timeRange">The time range to search for appointments</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePartialAppointments">Whether to include appointments that partially overlap with the specified
    /// time range. If false, only appointments that are completely within the time range will be included.</param>
    /// <returns></returns>
    public async Task<Result<List<Appointment>>> GetByVetIdAndTimeRangeAsync(
        int vetId,
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false
    )
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.GetByVetAndTimeRangeAsync(
                    vetId,
                    timeRange,
                    filters,
                    includePartialAppointments
                ),
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

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<int>> PopulateAppointments()
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

            // Get vets from db to know for which vets we will populate the schedule
            var vetsResult = await _vetRepo.GetVetsAsync();
            if (!vetsResult || vetsResult.Data is null)
                return new Result<int>
                {
                    Success = false,
                    Code = "ERROR_GETTING_VETS",
                    Status = 500,
                    Message = "An error occurred while getting vets",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var vets = vetsResult.Data;
            if (vets.Count == 0)
                return new Result<int>
                {
                    Success = false,
                    Code = "NO_VETS_FOUND",
                    Status = 404,
                    Message = "No vets were found to populate the schedule for",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var start = DateTime.UtcNow;
            var end = DateTime.UtcNow.AddDays(daysToPopulateAppointments);
            var timeRange = new TimeRange
            {
                Start = start,
                End = end
            };

            // Iterate each vet
            var totalAppointmentsAdded = 0;
            var anyError = false;
            foreach (var vet in vets)
            {
                var populateResult = await PopulateScheduleForVetAsync(vet, timeRange, appointmentDurationInMinutes);
                if (populateResult)
                {
                    totalAppointmentsAdded += populateResult.Data;
                    continue;
                }

                populateResult.Log(_logger);
                anyError = true;
            }

            return anyError
                ? new Result<int>
                {
                    Success = false,
                    Code = "ERROR_POPULATING_SCHEDULE_FOR_ONE_OR_MORE_VETS",
                    Status = 500,
                    Message =
                        "An error occurred while populating the schedule for one or more vets. Check logs for more details.",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = totalAppointmentsAdded
                }
                : new Result<int>
                {
                    Success = true,
                    Code = "SCHEDULE_POPULATED_SUCCESSFULLY",
                    Status = 200,
                    Message =
                        $"Schedule populated successfully for all vets. Total appointments added: {totalAppointmentsAdded}",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = totalAppointmentsAdded
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error accepting pet ownership");
            return new Result<int>
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

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<int>> PopulateScheduleForVetAsync(Vet vet, TimeRange timeRange, int appointmentDuration)
    {
        try
        {
            var existingResult = await GetByVetIdAndTimeRangeAsync(vet.Id, timeRange);
            if (!existingResult)
                return new Result<int>
                {
                    Success = false,
                    Code = "ERROR_GETTING_EXISTING_APPOINTMENTS",
                    Status = 500,
                    Message = "An error occurred while getting existing appointments",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (existingResult.Code == "NO_APPOINTMENTS_FOUND")
                return await PopulateDayForVet(vet, DateTime.UtcNow.Date, appointmentDuration);

            return new Result<int>
            {
                Success = false,
                Code = "DEVELOPING",
                Status = 200,
                Message = "Developing...",
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting existing appointments by time range");
            return new Result<int>
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
}