using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

public class PetService(
    IPetRepository petRepo,
    ILogger<PetService> logger
) : IPetService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The pet repository used to access and manage pets in the application. It is injected into the service to provide
    /// access to the underlying data source for pet-related operations, such as creating, updating, deleting, and
    /// retrieving pets. The service uses the repository to perform the necessary data access logic for pet management.
    /// </summary>
    private readonly IPetRepository _petRepo = petRepo;

    /// <summary>
    /// The logger used to log messages.
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
    /// <summary>
    /// Adds a new pet to the db
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    private async Task<Result<Pet?>> AddAsync(Pet pet)
    {
        try
        {
            _logger.LogInformation("Adding new pet");

            var addResult = await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.AddAsync(pet),
                operationName: "Adding new pet",
                logger: _logger
            );

            return addResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding new pet");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_ADDING_PET",
                Status = 500,
                Message = "An error occurred while adding the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a new pet in the system. It takes the pet data and the user id of the owner. It returns a result with
    /// the created pet or an error if something went wrong.
    /// </summary>
    /// <param name="request">The data of the pet to create</param>
    /// <returns>The created pet or an error if something went wrong</returns>
    public async Task<Result<Pet?>> CreatePetAsync(CreatePetRequest request)
    {
        try
        {
            _logger.LogInformation("Creating pet with name {PetName}", request.Name);

            // Verifications
            var requestResult = CheckCreatePetRequest(request);
            if (!requestResult) return requestResult.ConvertTo<Pet?>();

            // TODO check for existing with the same name and species pets for that user
            // Create the pet
            var pet = new Pet
            {
                Name = request.Name,
                Species = request.Species,
                Breed = request.Breed,
                Status = PetStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the pet
            var addResult = await AddAsync(pet);
            if (!addResult || addResult.Data is null) return addResult;
            pet = addResult.Data;

            _logger.LogInformation("Pet with name {PetName} created successfully with id {PetId}", pet.Name, pet.Id);
            // Return this cute, little pet
            return new Result<Pet?>
            {
                Success = true,
                Code = "PET_CREATED",
                Status = 201,
                Message = "Pet created successfully",
                Data = pet,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding new pet");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_ADDING_PET",
                Status = 500,
                Message = "An error occurred while adding the pet. Please try again later."
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a create pet request
    /// </summary>
    /// <param name="request">The crate pet request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    public Result CheckCreatePetRequest(CreatePetRequest request)
    {
        // Validations
        if (string.IsNullOrEmpty(request.Name))
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_NAME",
                Status = 400,
                Message = "The pet name is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.Name.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_NAME",
                Status = 400,
                Message = "The pet name must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.Breed.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_BREED",
                Status = 400,
                Message = "The pet breed must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (!Enum.IsDefined(typeof(PetSpecies), request.Species))
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_SPECIES",
                Status = 400,
                Message = "The pet species is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        return new Result
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Message = "Pet added successfully",
            TraceCode = FileCodes.CallerIC(),
            Returnable = false
        };
    }
}