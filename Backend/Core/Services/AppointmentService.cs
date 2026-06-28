using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Relationships;
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
    IPetRepository petRepo,
    INotificationService notificationService,
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
    /// Provides functionality to access and manage pets.
    /// </summary>
    private readonly IPetRepository _petRepo = petRepo;

    /// <summary>
    /// Provides functionality to send notifications to users.
    /// </summary>
    private readonly INotificationService _notificationService = notificationService;

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
                    Title = "An error occurred while getting existing appointments",
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
                    Title =
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
                DateTimeKind.Unspecified
            );

            // Start populating
            var appointments = new List<Appointment>();
            while (TimeOnly.FromDateTime(appointmentStart) < activeHours.End)
            {
                // Check for appointments inside active hours
                // because, even when we are so abusive, we
                // don't want to be sued xD
                var appointmentEnd = appointmentStart.AddMinutes(appointmentDuration);
                if (
                    TimeOnly.FromDateTime(appointmentStart) < activeHours.Start ||
                    TimeOnly.FromDateTime(appointmentEnd) > activeHours.End
                ) break;

                // Check for existing appointments
                if (
                    existingAppointments.Any(a =>
                        (appointmentStart >= a.StartTime && appointmentStart < a.EndTime) ||
                        (appointmentEnd > a.StartTime && appointmentEnd <= a.EndTime) ||
                        (appointmentStart <= a.StartTime && appointmentEnd >= a.EndTime)
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
                    StartTime = TimeZoneInfo.ConvertTimeToUtc(
                        appointmentStart,
                        Env.GetClinicTimeZone()
                    ),
                    EndTime = TimeZoneInfo.ConvertTimeToUtc(
                        appointmentEnd,
                        Env.GetClinicTimeZone()
                    ),
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
                    Title = $"An error occurred while adding new appointments for vet {vet.Id}",
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
                    Title =
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
                    Title =
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
                Title =
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
                Title = "The provided date is invalid",
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
                        Title = "The provided date is invalid",
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
                    Title = "The provided date is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }

        // Start with local date because user can be anywhere
        var localDay = new DateTime(
            request.Year,
            request.Month,
            request.Day,
            useCurrentTime ? DateTime.UtcNow.Hour : 0,
            useCurrentTime ? DateTime.UtcNow.Minute : 0,
            useCurrentTime ? DateTime.UtcNow.Second : 0,
            DateTimeKind.Unspecified
        );

        // Then I convert it to Utc so I can find things in my db
        return TimeZoneInfo.ConvertTimeToUtc(localDay, Env.GetClinicTimeZone());
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
                Title = "An error occurred while adding a new appointment",
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
                Title = "An error occurred while adding new appointments",
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
                Title = "An error occurred while getting existing appointments by time range",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="appointmentId">The appointment ID to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeVet">Whether to include the vet in the response</param>
    /// <param name="includeUser">Whether to include the user in the response</param>
    /// <param name="includePet">Whether to include the pet in the response</param>
    /// <returns></returns>
    public async Task<Result<Appointment?>> GetByIdAsync(
        int appointmentId,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includeUser = false,
        bool includePet = false
    )
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () =>
                    _appointmentsRepo.GetByIdAsync(appointmentId, filters, includeVet, includeUser, includePet),
                operationName: "getting appointment by id",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting appointment by id");
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_GETTING_APPOINTMENT_BY_ID",
                Status = 500,
                Title = "An error occurred while getting the appointment by id",
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
                Title = "An error occurred while getting existing appointments by time range",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
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
                    Title = "An error occurred while getting vets",
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
                    Title = "No vets were found to populate the schedule for",
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
                    Title =
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
                    Title =
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
                Title = "An error occurred while accepting pet ownership",
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
            var dayResult = CreateDay(request);
            if (!dayResult) return dayResult.ConvertTo<List<Appointment>>();
            var day = dayResult.Data;

            // Day validations
            if (day < DateTime.UtcNow.Date)
                return new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "CANNOT_GET_AVAILABLE_APPOINTMENTS_FOR_PAST_DAYS",
                    Status = 400,
                    Title = "Cannot get available appointments for past days",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (day.DayOfWeek == DayOfWeek.Sunday)
                return new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "CANNOT_GET_AVAILABLE_APPOINTMENTS_FOR_SUNDAY",
                    Status = 400,
                    Title = "Cannot get available appointments for Sunday as the vets do not work on Sundays (yet).",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var rage = new TimeRange
            {
                Start = day,
                End = day.AddDays(1)
            };

            // Search for those appointments
            var result = await DbRetry.ExecuteWithRetry(
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
            if (
                !result ||
                result.Data is null ||
                result.Data.Count == 0
            )
                return new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "NO_AVAILABLE_APPOINTMENTS_FOUND",
                    Status = 404,
                    Title = "No available appointments were found for the specified day",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var appointments = result.Data.Where(a => a.StartTime > DateTime.UtcNow.AddMinutes(15)).ToList();
            return appointments.Count == 0
                ? new Result<List<Appointment>>
                {
                    Success = false,
                    Code = "NO_AVAILABLE_APPOINTMENTS_FOUND",
                    Status = 404,
                    Title = "No available appointments were found for the specified day",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : new Result<List<Appointment>>
                {
                    Success = true,
                    Code = "AVAILABLE_APPOINTMENTS_FOUND",
                    Status = 200,
                    Title =
                        $"Available appointments were found for the specified day. Total available appointments: {appointments.Count}",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true,
                    Data = appointments
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting all vets");
            return new Result<List<Appointment>>
            {
                Success = false,
                Code = "ERROR_GETTING_APPOINTMENTS",
                Status = 500,
                Title = "An error occurred while getting the appointments",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
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
    public async Task<Result<Appointment?>> ReserveAppointmentAsync(ReserveAppointmentRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Reserving appointment with id {AppointmentId} for user with id {UserId} and pet with id {PetId}",
                request.AppointmentId, request.UserId, request.PetId);

            // Get appointment
            var filters = StatusFilters.IncludeAll();
            var appointmentResul = await GetByIdAsync(request.AppointmentId, filters, true);
            if (!appointmentResul || appointmentResul.Data is null)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "APPOINTMENT_NOT_FOUND",
                    Status = 404,
                    Title = "The appointment was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var appointment = appointmentResul.Data;

            switch (appointment.Status)
            {
                // Validations
                case AppointmentStatus.Scheduled:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "APPOINTMENT_ALREADY_SCHEDULED",
                        Status = 400,
                        Title = "The appointment is already scheduled",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case AppointmentStatus.Completed:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "APPOINTMENT_ALREADY_COMPLETED",
                        Status = 400,
                        Title = "The appointment is already completed",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            if (appointment.StartTime < DateTime.UtcNow)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "APPOINTMENT_IN_PAST",
                    Status = 400,
                    Title = "The appointment is in the past and cannot be reserved",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (appointment.StartTime <= DateTime.UtcNow.AddMinutes(15))
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "APPOINTMENT_TOO_CLOSE",
                    Status = 400,
                    Title =
                        "The appointment is too close to be reserved. Appointments must be reserved at least 15 minutes in advance.",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Vet
            if (appointmentResul.Data is null)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "VET_NOT_FOUND",
                    Status = 404,
                    Title = "The vet was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Get user pet relationship
            var userPet = await _petRepo.GetUserPetByBothIdsAsync(
                request.UserId,
                request.PetId,
                filters,
                includeUser: true,
                includePet: true
            );

            if (!userPet || userPet.Data is null)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "USER_PET_NOT_FOUND",
                    Status = 404,
                    Title = "The user pet was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check pet status
            if (userPet.Data.Pet is null)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "PET_NOT_FOUND",
                    Status = 404,
                    Title = "The pet was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var pet = userPet.Data.Pet;

            switch (pet.Status)
            {
                case EntityStatus.Inactive:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "PET_NOT_ACTIVE",
                        Status = 400,
                        Title = "The pet is not active and cannot be scheduled for an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Deleted:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "PET_DELETED",
                        Status = 400,
                        Title = "The pet is deleted and cannot be scheduled for an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Archived:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "PET_ARCHIVED",
                        Status = 400,
                        Title = "The pet is archived and cannot be scheduled for an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Banned:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "PET_BANNED",
                        Status = 400,
                        Title = "The pet is banned and cannot be scheduled for an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            // Check user
            if (userPet.Data.User is null)
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Title = "The user was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userPet.Data.User;

            switch (user.Status)
            {
                case EntityStatus.Inactive:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "USER_NOT_ACTIVE",
                        Status = 400,
                        Title = "The user is not active and cannot reserve an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Deleted:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "USER_DELETED",
                        Status = 400,
                        Title = "The user is deleted and cannot reserve an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Archived:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "USER_ARCHIVED",
                        Status = 400,
                        Title = "The user is archived and cannot reserve an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Banned:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "USER_BANNED",
                        Status = 400,
                        Title = "The user is banned and cannot reserve an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Unverified:
                    return new Result<Appointment?>
                    {
                        Success = false,
                        Code = "USER_UNVERIFIED",
                        Status = 400,
                        Title = "The user is unverified and cannot reserve an appointment",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            // Update appointment
            appointment.UserPetId = userPet.Data.Id;
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.UpdatedAt = DateTime.UtcNow;
            var result = await UpdateAsync(appointment);
            if (!result || result.Data is null)
            {
                result.Log(_logger, "Error reserving appointment");
                return new Result<Appointment?>
                {
                    Success = false,
                    Code = "ERROR_RESERVING_APPOINTMENT",
                    Status = 500,
                    Title = "An error occurred while reserving the appointment",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            // Appointment reserved, so send the email confirming the appointment
            appointment.UserPet = userPet.Data;
            var emailResult = await _notificationService.SendAppointmentConfirmationEmailAsync(appointment);
            if (!emailResult)
                return new Result<Appointment?>
                {
                    Success = true,
                    Code = "ERROR_SENDING_APPOINTMENT_CONFIRMATION_EMAIL",
                    Status = 200,
                    Title =
                        "Appointment reserved successfully, but an error occurred while sending the confirmation email",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            return new Result<Appointment?>
            {
                Success = true,
                Code = "APPOINTMENT_RESERVED_SUCCESSFULLY",
                Status = 200,
                Title = "The appointment was reserved successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true,
                Data = result.Data
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error reserving appointment");
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_RESERVING_APPOINTMENT",
                Status = 500,
                Title = "An error occurred while reserving the appointment"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<Appointment?>> UpdateAsync(Appointment appointment)
    {
        try
        {
            return await DbRetry.ExecuteWithRetry(
                operation: () => _appointmentsRepo.UpdateAsync(appointment),
                operationName: "updating appointment",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error updating appointment");
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_UPDATING_APPOINTMENT",
                Status = 500,
                Title = "An error occurred while updating the appointment",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<UserPet?>> GetUserPetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUser = false,
        bool includePet = false
    )
    {
        try
        {
            _logger.LogInformation("Getting user pet by id {UserPetId}", id);

            if (id <= 0)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "INVALID_USER_PET_ID",
                    Status = 400,
                    Title = "The user pet ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var getResult = await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.GetUserPetByIdAsync(id, filters, includeUser, includePet),
                operationName: $"Getting user pet by id {id}",
                logger: _logger
            );

            if (!getResult) return getResult;
            if (getResult.Data is null)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "USER_PET_NOT_FOUND",
                    Status = 404,
                    Title = "User pet not found"
                };

            _logger.LogInformation("User pet with id {UserPetId} retrieved successfully", id);
            return getResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error getting user pet by id {id}");
            return new Result<UserPet?>
            {
                Success = false,
                Code = "ERROR_GETTING_USER_PET",
                Status = 500,
                Title = "An error occurred while getting the user pet. Please try again later.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}