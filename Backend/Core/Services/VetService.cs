using Backend.Core.Internal;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Models.Vets;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

public class VetService(
    IVetRepository vetRepository,
    ILogger<VetService> logger
    ) : IVetService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    private readonly IVetRepository _vetRepo = vetRepository;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<VetService> _logger = logger;


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
    public async Task<Result<List<Vet>>> GetVetsAsync(StatusFilters? filters = null)
    {
        try
        {
            _logger.LogInformation("Getting all vets");

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _vetRepo.GetVetsAsync(),
                operationName: "Getting all vets",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting all vets");
            return new Result<List<Vet>>
            {
                Success = false,
                Code = "ERROR_GETTING_VETS",
                Status = 500,
                Message = "An error occurred while getting the vets",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}