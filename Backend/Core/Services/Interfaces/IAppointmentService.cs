using Backend.Core.Models.Appointments;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Models.Vets;

namespace Backend.Core.Services.Interfaces;

public interface IAppointmentService
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
    public Task<Result<Appointment?>> AddAsync(Appointment appointment);

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
    public Task<Result<List<Appointment>>> GetByVetIdAndTimeRangeAsync(
        int vetId,
        TimeRange timeRange,
        StatusFilters? filters = null,
        bool includePartialAppointments = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<int>> PopulateAppointments();
}