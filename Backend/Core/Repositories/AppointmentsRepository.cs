using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

/// <remarks>FA03</remarks>
public class AppointmentsRepository(
    ApplicationDbContext dbContext,
    ILogger<PetRepository> logger
) : IAppointmentsRepository
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The database context used to interact with the database.
    /// This context is used to perform CRUD operations on the <see cref="User"/> and
    /// <see cref="DocumentType"/> entities.
    /// It is injected into the repository to provide access to the underlying database.
    /// </summary>
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<PetRepository> _logger = logger;

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
    private static IQueryable<Appointment> ApplyStatusFilters(
        IQueryable<Appointment> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeAvailable) query = query.Where(i => i.Status != AppointmentStatus.Available);
        if (!filters.IncludeScheduled) query = query.Where(i => i.Status != AppointmentStatus.Scheduled);
        if (!filters.IncludeCompleted) query = query.Where(i => i.Status != AppointmentStatus.Completed);
        if (!filters.IncludeCanceled) query = query.Where(i => i.Status != AppointmentStatus.Canceled);

        return query;
    }


    //                                                                                                    Public Methods
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
        // Find appointments
        var query = includePartialAppointments
            ? _dbContext.Appointments.Where(a => a.EndTime >= timeRange.Start && a.StartTime <= timeRange.End)
            : _dbContext.Appointments.Where(a => a.StartTime >= timeRange.Start && a.EndTime <= timeRange.End);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .AsSplitQuery();

        // Execute query
        var appointments = await query.ToListAsync();

        return new Result<List<Appointment>>
        {
            Success = true,
            Code = appointments.Count != 0 ? "APPOINTMENTS_FOUND" : "NO_APPOINTMENTS_FOUND",
            Status = 200,
            Title = appointments.Count != 0
                ? "Appointments found successfully"
                : "No appointments found between the specified start and end times",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true,
            Data = new List<Appointment>()
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="vetId"></param>
    /// <param name="timeRange">The time range to search for appointments</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePartialAppointments">Whether to include appointments that partially overlap with the specified
    ///     time range. If false, only appointments that are completely within the time range will be included.</param>
    /// <returns></returns>
    public async Task<Result<List<Appointment>>> GetByVetAndTimeRangeAsync(
        int vetId,
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false)
    {
        // Find appointments
        var query = includePartialAppointments
            ? _dbContext.Appointments.Where(a => a.EndTime >= timeRange.Start && a.StartTime <= timeRange.End)
            : _dbContext.Appointments.Where(a => a.StartTime >= timeRange.Start && a.EndTime <= timeRange.End);

        query = query
            .Where(a => a.VetId == vetId);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        query = query
            .AsSplitQuery();

        // Execute query
        var appointments = await query.ToListAsync();

        return new Result<List<Appointment>>
        {
            Success = true,
            Code = appointments.Count != 0 ? "APPOINTMENTS_FOUND" : "NO_APPOINTMENTS_FOUND",
            Status = 200,
            Title = appointments.Count != 0
                ? "Appointments found successfully"
                : "No appointments found between the specified start and end times",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true,
            Data = new List<Appointment>()
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<Appointment?>> AddAsync(Appointment appointment)
    {
        // Validations
        if (appointment.VetId <= 0)
        {
            return new Result<Appointment?>
            {
                Success = false,
                Code = "INVALID_VET_ID",
                Status = 400,
                Title = "Invalid veterinarian ID",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true,
                Data = null
            };
        }

        // Add the entity
        _dbContext.Appointments.Add(appointment);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_CREATING_PET",
                Status = 500,
                Title = "An error occurred while creating the pet",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Get the appointment back from the db
        var getAppointmentResult = await GetByIdAsync(appointment.Id);
        if (!getAppointmentResult || getAppointmentResult.Data == null)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "APPOINTMENT_CREATED_BUT_NOT_FOUND",
                Status = 500,
                Title = "Appointment created but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<Appointment?>
        {
            Success = true,
            Code = "APPOINTMENT_CREATED",
            Status = 201,
            Title = "Appointment created successfully",
            Data = getAppointmentResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<int>> AddRangeAsync(List<Appointment> appointments)
    {
        foreach (var appointment in appointments)
        {
            // Validations
            if (appointment.VetId <= 0)
                return new Result<int>
                {
                    Success = false,
                    Code = "INVALID_VET_ID",
                    Status = 400,
                    Title = "Invalid veterinarian ID",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }

        // Save those entities
        _dbContext.Appointments.AddRange(appointments);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<int>
            {
                Success = false,
                Code = "ERROR_CREATING_APPOINTMENTS",
                Status = 500,
                Title = "An error occurred while creating the appointments",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<int>
        {
            Success = true,
            Code = "APPOINTMENTS_CREATED",
            Status = 201,
            Title = "Appointments created successfully",
            Data = saved,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }


    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets an appointment by its ID.
    /// </summary>
    /// <param name="id">The ID of the appointment to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeVet">Whether to include the vet data in the query</param>
    /// <param name="includePet">Whether to include the pet data in the query</param>
    /// <returns>A <see cref="Result{Appointment}"/> indicating the result of the operation and including the appointment if it was found</returns>
    public async Task<Result<Appointment?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includePet = false
    )
    {

        if (id <= 0)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "INVALID_APPOINTMENT_ID",
                Status = 400,
                Title = "The provided appointment ID is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Find the appointment
        var query = _dbContext.Appointments
            .Where(a => a.Id == id);

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeVet)
            query = query
                .Include(a => a.Vet);

        if (includePet)
            query = query
                .Include(p => p.Pet);

        query = query.AsSplitQuery();

        // Execute query
        var appointment = await query.FirstOrDefaultAsync();
        return appointment is null
            ? new Result<Appointment?>
            {
                Success = false,
                Code = "APPOINTMENT_NOT_FOUND",
                Status = 404,
                Title = "No appointment found with the provided id",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            }
            : new Result<Appointment?>
            {
                Success = true,
                Code = "APPOINTMENT_FOUND",
                Status = 200,
                Title = "Appointment found successfully",
                Data = appointment,
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<List<Appointment>>> GetAvailableAppointmentsAsync(
        TimeRange range,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includePet = false
    )
    {
        // Find appointments
        var query = _dbContext.Appointments.AsQueryable()
            .Where(a =>
                a.Status == AppointmentStatus.Available &&
                a.StartTime >= range.Start &&
                a.EndTime <= range.End
            );

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        // Includes
        if (includeVet)
            query = query
                .Include(a => a.Vet);

        if (includePet)
            query = query
                .Include(p => p.Pet);

        query = query.AsSplitQuery();

        // Execute query
        var appointments = await query.ToListAsync();

        return new Result<List<Appointment>>
        {
            Success = true,
            Code = appointments.Count != 0 ? "APPOINTMENTS_FOUND" : "NO_APPOINTMENTS_FOUND",
            Status = 200,
            Title = appointments.Count != 0
                ? "Appointments found successfully"
                : "No appointments found",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true,
            Data = appointments
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<Appointment?>> UpdateAsync(Appointment appointment)
    {
        if (appointment.Id <= 0)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "INVALID_APPOINTMENT_ID",
                Status = 400,
                Title = "The provided appointment ID is invalid",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Update the entity
        _dbContext.Appointments.Update(appointment);
        var saved = await _dbContext.SaveChangesAsync();
        if (saved <= 0)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "ERROR_UPDATING_APPOINTMENT",
                Status = 500,
                Title = "An error occurred while updating the appointment",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        // Get the appointment back from the db
        var filters = StatusFilters.IncludeAll();
        var getAppointmentResult = await GetByIdAsync(appointment.Id, filters);
        if (!getAppointmentResult || getAppointmentResult.Data == null)
            return new Result<Appointment?>
            {
                Success = false,
                Code = "APPOINTMENT_UPDATED_BUT_NOT_FOUND",
                Status = 500,
                Title = "Appointment updated but not found when retrieving it",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = true
            };

        return new Result<Appointment?>
        {
            Success = true,
            Code = "APPOINTMENT_UPDATED",
            Status = 200,
            Title = "Appointment updated successfully",
            Data = getAppointmentResult.Data,
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = true
        };
    }
}