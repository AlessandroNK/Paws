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
            // Creating day time range to check for existing
            var range = new TimeRange
            {
                Start = day,
                End = day.AddDays(1)
            };
            var existingResult = await GetByVetIdAndTimeRangeAsync(vet.Id, range);
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
            var existingAppointments = existingResult.Data ?? new List<Appointment>();

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
            var appointmentStart = new DateTime(
                day.Year,
                day.Month,
                day.Day,
                activeHours.Start.Hour,
                activeHours.Start.Minute,
                0,
                DateTimeKind.Utc
            );

            // Start populating
            var appointments = new List<Appointment>();
            while (TimeOnly.FromDateTime(appointmentStart) < activeHours.End)
            {
                // Check for appointments inside active hours
                // because, even when we are so abusive, we
                // don't want to be sued xD
                var endTime = appointmentStart.AddMinutes(appointmentDuration);
                if (
                    TimeOnly.FromDateTime(appointmentStart) < activeHours.Start ||
                    TimeOnly.FromDateTime(endTime) > activeHours.End
                ) break;

                // Check for existing appointments
                if (
                    existingAppointments.Any(a =>
                        (appointmentStart >= a.StartTime && appointmentStart < a.EndTime) ||
                        (endTime > a.StartTime && endTime <= a.EndTime) ||
                        (appointmentStart <= a.StartTime && endTime >= a.EndTime)
                    )
                )
                {
                    // If there is an existing appointment that
                    // overlaps with the current appointment date,
                    // we skip it. ANd yes, I almost lose my mind here
                    appointmentStart = appointmentStart.AddMinutes(appointmentDuration);
                    continue;
                }

                // If it fits, then add it
                var appointment = new Appointment
                {
                    EndTime = endTime,
                    StartTime = appointmentStart,
                    Status = AppointmentStatus.Available,
                    UserPetId = null,
                    VetId = vet.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save the appointment
                appointments.Add(appointment);
                appointmentStart = appointmentStart.AddMinutes(appointmentDuration);
            }

            // Save them
            var result = await AddRangeAsync(appointments);
            if (!result)
                return new Result<int>
                {
                    Success = false,
                    Code = "ERROR_ADDING_NEW_APPOINTMENTS",
                    Status = 500,
                    Message = $"An error occurred while adding new appointments for vet {vet.Id}",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var errors = appointments.Count - result.Data;

            // Finished!!
            return errors == 0
                ? new Result<int>
                {
                    Success = true,
                    Code = "SCHEDULE_POPULATED_SUCCESSFULLY_FOR_VET",
                    Status = 200,
                    Message =
                        $"Schedule populated successfully for vet {vet.Id}. Total appointments added: {result.Data}",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = result.Data
                }
                : new Result<int>
                {
                    Success = false,
                    Code = "SCHEDULE_POPULATED_WITH_ERRORS_FOR_VET",
                    Status = 500,
                    Message =
                        $"Schedule populated for vet {vet.Id} with {errors} errors. Total appointments added: {result.Data}. Check logs for more details.",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = result.Data
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

    // -----------------------------------------------------------------------------------------------------------------
    private static Result<DateTime> CreateDay(AppointmetDayRequest request, bool useCurrentTime = false)
    {
        if (request.Day < 1 || request.Day > 31 || request.Month < 1 || request.Month > 12 || request.Year < 1)
            return new Result<DateTime>
            {
                Success = false,
                Code = "INVALID_DATE",
                Status = 400,
                Message = "The provided date is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        switch (request.Month)
        {
            case 2:
            {
                var isLeapYear = DateTime.IsLeapYear(request.Year);
                if (request.Day > 29 || (request.Day == 29 && !isLeapYear))
                    return new Result<DateTime>
                    {
                        Success = false,
                        Code = "INVALID_DATE",
                        Status = 400,
                        Message = "The provided date is invalid",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                break;
            }
            case 4 or 6 or 9 or 11 when request.Day > 30:
                return new Result<DateTime>
                {
                    Success = false,
                    Code = "INVALID_DATE",
                    Status = 400,
                    Message = "The provided date is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }

        return new DateTime(
            request.Year,
            request.Month,
            request.Day,
            useCurrentTime ? DateTime.Now.Hour : 0,
            useCurrentTime ? DateTime.Now.Minute : 0,
            useCurrentTime ? DateTime.Now.Second : 0,
            DateTimeKind.Utc
        );
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
    public async Task<Result<int>> AddRangeAsync(List<Appointment> appointments)
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.AddRangeAsync(appointments),
                operationName: "adding new appointments",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding new appointments");
            return new Result<int>
            {
                Success = false,
                Code = "ERROR_ADDING_NEW_APPOINTMENTS",
                Status = 500,
                Message = "An error occurred while adding new appointments",
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
    public async Task<Result<int>> PopulateAppointments(AppointmetDayRequest request)
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

            // Creating day.
            var dayResult = CreateDay(request);
            if (!dayResult) return dayResult.ConvertTo<int>();
            var day = dayResult.Data;


            // Iterate each vet
            var totalAppointmentsAdded = 0;
            var anyError = false;
            foreach (var vet in vets)
            {
                var populateResult = await PopulateDayForVet(vet, day, appointmentDurationInMinutes);
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
    public async Task<Result<List<Appointment>>> GetAvailableAppointmentsAsync(
        AppointmetDayRequest request,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includeUser = false,
        bool includePet = false
    )
    {
        try
        {
            _logger.LogInformation("Getting all vets");

            // Creating day.
            var dayResult = CreateDay(request, true);
            if (!dayResult) return dayResult.ConvertTo<List<Appointment>>();
            var day = dayResult.Data;

            // Day validations
            if (day < DateTime.UtcNow.Date)
                return new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "CANNOT_GET_AVAILABLE_APPOINTMENTS_FOR_PAST_DAYS",
                    Status = 400,
                    Message = "Cannot get available appointments for past days",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (day.DayOfWeek == DayOfWeek.Sunday)
                return new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "CANNOT_GET_AVAILABLE_APPOINTMENTS_FOR_SUNDAY",
                    Status = 400,
                    Message = "Cannot get available appointments for Sunday as the vets do not work on Sundays (yet).",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var rage = new TimeRange
            {
                Start = day,
                End = day.AddDays(1)
            };

            // Search for those appointments
            return await DbRetry.ExecuteWithRetry(
                operation: () =>
                    _appointmentsRepo.GetAvailableAppointmentsAsync(
                        rage,
                        filters,
                        includeVet,
                        includeUser,
                        includePet
                    ),
                operationName: "Getting all vets",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting all vets");
            return new Result<List<Appointment>>
            {
                Success = false,
                Code = "ERROR_GETTING_APPOINTMENTS",
                Status = 500,
                Message = "An error occurred while getting the appointments",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}