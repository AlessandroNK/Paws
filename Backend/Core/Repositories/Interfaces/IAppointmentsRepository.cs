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
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public Task<Result<List<Appointment>>> GetByTimeRangeAsync(
        DateTime start,
        DateTime end,
        StatusFilters? filters = null
    );
}