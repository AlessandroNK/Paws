using Backend.Core.Models.Appointments;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;

namespace Backend.Core.Repositories.Interfaces;

public interface IAppointmentsRepository
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
    /// <summary>
    /// Gets appointments that are scheduled between the specified start and end times.
    /// </summary>
    /// <param name="timeRange">The time range to search for appointments</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includePartialAppointments">Whether to include appointments that partially overlap with the specified
    /// time range. If false, only appointments that are completely within the time range will be included.</param>
    /// <returns></returns>
    public Task<Result<List<Appointment>>> GetByTimeRangeAsync(
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false
    );

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
    public Task<Result<List<Appointment>>> GetByVetAndTimeRangeAsync(
        int vetId,
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<Appointment?>> AddAsync(Appointment appointment);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<int>> AddRangeAsync(List<Appointment> appointments);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets an appointment by its ID.
    /// </summary>
    /// <param name="id">The ID of the appointment to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeVet">Whether to include the vet data in the query</param>
    /// <param name="includeUser">Whether to include the user pet data in the query</param>
    /// <param name="includePet">Whether to include the pet data in the query</param>
    /// <returns>A <see cref="Result{Appointment}"/> indicating the result of the operation and including the appointment if it was found</returns>
    public Task<Result<Appointment?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includeUser = false,
        bool includePet = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<List<Appointment>>> GetAvailableAppointmentsAsync(
        TimeRange range,
        StatusFilters? filters = null,
        bool includeVet = false,
        bool includeUser = false,
        bool includePet = false
    );
}