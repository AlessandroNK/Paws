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

        if (!filters.IncludeFree) query = query.Where(i => i.Status != AppointmentStatus.Free);
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
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public async Task<Result<List<Appointment>>> GetByTimeRangeAsync(
        DateTime start,
        DateTime end,
        StatusFilters? filters = null
    )
    {
        // Find appointments
        var query = _dbContext.Appointments
            .Where(a => a.Start >= start && a.End <= end);

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
            Message = appointments.Count != 0 ? "Appointments found successfully" : "No appointments found between the specified start and end times",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true,
            Data = new List<Appointment>()
        };
    }
}