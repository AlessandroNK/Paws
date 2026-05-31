using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

public class PetService(
    IPetRepository petRepo,
    INotificationService notificationService,
    IUserRepository userRepo,
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
    /// Provides functionality to access and manage users in the application.
    /// This interface is responsible for managing users in the application.
    /// It provides methods for creating, updating, deleting, and retrieving users.
    /// The implementation of this interface should handle the data access logic, such as interacting with a database
    /// or any other data source.
    /// </summary>
    private readonly IUserRepository _userRepo = userRepo;

    /// <summary>
    /// The service to notify users. It is injected into the service to provide access to the underlying notification
    /// system for sending notifications to users. The service uses the notification service to send relevant notifications
    /// to users, such as verification codes, password reset instructions, and other important updates related to their
    /// accounts. Those notifications can be sent by frontend, email or sms.
    /// </summary>
    private readonly INotificationService _notificationService = notificationService;

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
    private static Result CheckSharePetOwnershipRequest(SharePetOwnershipRequest request)
    {
        if (request.UserId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_USER_ID",
                Status = 400,
                Message = "The user ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.PetId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_ID",
                Status = 400,
                Message = "The pet ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (string.IsNullOrEmpty(request.NewOwnerEmail))
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "The email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.NewOwnerEmail.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "The email must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        var emailResult = SecurityService.ValidateEmailAddress(request.NewOwnerEmail);
        if (!emailResult) return emailResult;

        return new Result
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Message = "Share pet ownership request is valid",
            TraceCode = FileCodes.CallerIC(),
            Returnable = false
        };
    }

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

            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.AddAsync(pet),
                operationName: "Adding new pet",
                logger: _logger
            );
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
    /// Gets a pet by its ID.
    /// </summary>
    /// <param name="id">The ID of the pet to retrieve</param>
    /// <param name="excludeInactive">Whether to exclude inactive pets</param>
    /// <param name="excludeBanned">Whether to exclude banned pets</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public async Task<Result<Pet?>> GetByIdAsync(int id, bool excludeInactive = true, bool excludeBanned = true)
    {
        try
        {
            _logger.LogInformation("Getting pet by id {PetId}", id);

            if (id <= 0)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "INVALID_PET_ID",
                    Status = 400,
                    Message = "The pet ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var getResult = await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.GetByIdAsync(id, excludeInactive, excludeBanned),
                operationName: $"Getting pet by id {id}",
                logger: _logger
            );

            if (!getResult) return getResult;
            if (getResult.Data is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "PET_NOT_FOUND",
                    Status = 404,
                    Message = "Pet not found"
                };

            _logger.LogInformation("Pet with id {PetId} retrieved successfully", id);
            return getResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error getting pet by id {id}");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_GETTING_PET",
                Status = 500,
                Message = "An error occurred while getting the pet. Please try again later."
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates a pet in the system. It takes the pet data and returns a result with the updated pet or an error if
    /// something went wrong.
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    public async Task<Result<Pet?>> UpdateAsync(Pet pet)
    {
        try
        {
            _logger.LogInformation("Updating pet with id {PetId}", pet.Id);

            if (pet.Id <= 0)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "INVALID_PET_ID",
                    Status = 400,
                    Message = "The pet ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var updateResult = await _petRepo.UpdateAsync(pet);
            if (!updateResult || updateResult.Data is null) return updateResult;

            _logger.LogInformation("Pet with id {PetId} updated successfully", pet.Id);
            return updateResult;
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shares the ownership of a pet with another user by sending an invitation email with a code to the new owner. The
    /// new owner can then use the code to accept the invitation and become a co-owner of the pet. This method checks if
    /// the user is the owner of the pet, generates a share code, saves it in the database, and sends it to the new owner's
    /// email address. The share code is valid for 24 hours.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result> SendOwnershipInvitationAsync(SharePetOwnershipRequest request)
    {
        try
        {
            _logger.LogInformation("Sharing ownership of pet {PetId} with user {Email}", request.PetId,
                request.NewOwnerEmail);

            // Verifications
            var requestResult = CheckSharePetOwnershipRequest(request);
            if (!requestResult) return requestResult;

            // Check for pet existence
            var existenceResult = await GetByIdAsync(request.PetId);
            if (!existenceResult || existenceResult.Data is null) return existenceResult;
            var pet = existenceResult.Data;

            // Check for the pet, it has to be
            // owned by this specific owner
            var userPet = pet.UserPets.FirstOrDefault(u => u.User?.Id == request.UserId);
            if (userPet?.User is null)
                return new Result
                {
                    Success = false,
                    Code = "USER_NOT_OWNER",
                    Status = 403,
                    Message = "The user is not the owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var owner = userPet.User;

            // Check if the new owner is a user already or not
            // here, I will send a link for already registered
            // users so they can login and accept in only one
            // operation, improving UX. For new users, I will
            // send a link to signUp in the page easily, with
            // their email and name already wrote in the form
            var newOwnerResult = await _userRepo.GetByEmailAsync(request.NewOwnerEmail);
            var newOwner = newOwnerResult.Data ?? new User
            {
                Name = request.NewOwnerName,
                Email = request.NewOwnerEmail
            };

            // Check if there is another code that has not expired
            // yet, if not, then generate a new code and store it
            if (string.IsNullOrWhiteSpace(pet.ShareCode) || pet.ShareCodeExpiration <= DateTime.UtcNow)
            {
                pet.ShareCode = SecurityService.GenerateVerificationCode();
                pet.ShareCodeExpiration = DateTime.UtcNow.AddHours(24);
                var resultUpdate = await UpdateAsync(pet);
                if (!resultUpdate || resultUpdate.Data is null) return resultUpdate;
            }

            // Send the code through Email to the invited owner
            var notificationResult = await _notificationService.SendOwnershipShareCode(
                pet.Name,
                owner.Name,
                newOwner.Name,
                newOwner.Email,
                pet.ShareCode
            );
            if (!notificationResult) return notificationResult;

            _logger.LogInformation("Ownership code of pet {PetId} shared successfully with user {Email}", request.PetId,
                request.NewOwnerEmail);
            return new Result
            {
                Success = true,
                Code = "PET_OWNERSHIP_CODE_SENT",
                Status = 200,
                Message = "Pet ownership code sent successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error sharing pet ownership");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_SHARING_PET_OWNERSHIP",
                Status = 500,
                Message = "An error occurred while sharing pet ownership",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
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