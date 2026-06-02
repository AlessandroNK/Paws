using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

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
    public async Task<Result> PopulateAppointments()
    {
        try
        {

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