using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

/// <remarks>FP02</remarks>
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
    private static Result CheckSharePetOwnershipRequest(SendOwnershipInvitationRequest invitationRequest)
    {
        if (invitationRequest.UserId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_USER_ID",
                Status = 400,
                Title = "The user ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (invitationRequest.PetId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_ID",
                Status = 400,
                Title = "The pet ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (string.IsNullOrEmpty(invitationRequest.NewOwnerEmail))
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Title = "The email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (invitationRequest.NewOwnerEmail.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Title = "The email must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        var emailResult = SecurityService.ValidateEmailAddress(invitationRequest.NewOwnerEmail);
        if (!emailResult) return emailResult;

        return new Result
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Title = "Share pet ownership request is valid",
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
                Title = "An error occurred while adding the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a new ownership invitation for a pet. It takes the pet, the owner, the new owner's name and email, and
    /// returns a result with the created ownership invitation or an error if something went wrong. This method checks
    /// if the new owner is already a user in the system, creates an ownership invitation with a unique invitation codeand an
    /// expiration time of 24 hours, generates an ownership link for the invitation, saves it in the database, and returns
    /// the created invitation. The ownership link is generated by encrypting a payload containing the invitation data
    /// and appending it to a frontend URL. The new owner can then use the ownership link to accept the invitation and
    /// become a co-owner of the pet.
    /// </summary>
    /// <param name="pet"></param>
    /// <param name="owner"></param>
    /// <param name="newOwnerName"></param>
    /// <param name="newOwnerEmail"></param>
    /// <returns></returns>
    private async Task<Result<OwnershipInvitation>> CreateOwnershipInvitation(
        Pet pet,
        User owner,
        string newOwnerName,
        string newOwnerEmail
    )
    {
        // Check if the new owner is a user already or not
        // here, I will send a link for already registered
        // users so they can login and accept in only one
        // operation, improving UX. For new users, I will
        // send a link to signUp in the page easily, with
        // their email and name already wrote in the form
        var newOwnerResult = await GetByEmailAsync(newOwnerEmail);

        // Any error but user not found
        if (!newOwnerResult && newOwnerResult.Code != "USER_NOT_FOUND")
            return newOwnerResult.ConvertTo<OwnershipInvitation>();

        // Create based on the user
        var invitation = !newOwnerResult || newOwnerResult.Data is null
            ? new OwnershipInvitation
            {
                UserId = owner.Id,
                User = owner,
                PetId = pet.Id,
                Pet = pet,
                NewOwnerName = newOwnerName,
                NewOwnerEmail = newOwnerEmail,
                Expiration = DateTime.UtcNow.AddHours(24),
                NewOwnerHasAccount = false,
                InvitationCode = Guid.NewGuid().ToString(),
                Status = EntityStatus.Active
            }
            : new OwnershipInvitation
            {
                UserId = owner.Id,
                User = owner,
                PetId = pet.Id,
                Pet = pet,
                NewOwnerName = newOwnerResult.Data.Name,
                NewOwnerEmail = newOwnerResult.Data.Email,
                Expiration = DateTime.UtcNow.AddHours(24),
                NewOwnerHasAccount = true,
                InvitationCode = Guid.NewGuid().ToString(),
                Status = EntityStatus.Active
            };

        // Save and return this invitation
        var addResult = await AddOwnershipInvitationAsync(invitation);
        if (!addResult || addResult.Data is null)
            return new Result<OwnershipInvitation>
            {
                Success = false,
                Code = "ERROR_CREATING_OWNERSHIP_INVITATION",
                Status = 500,
                Title = "An error occurred while creating the ownership invitation",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        // Add important dependencies
        addResult.Data.Pet = pet;
        addResult.Data.User = owner;
        return addResult.ConvertTo<OwnershipInvitation>();
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates an ownership link for an ownership invitation. It takes an ownership invitation and returns a result with the generated
    /// ownership link or an error if something went wrong. This method generates an ownership link by encrypting a payload
    /// containing the invitation data and appending it to a frontend URL. The payload includes the user ID, pet ID,
    /// new owner's name and email, whether the new owner has an account, and a unique invitationCode. The ownership link is used by
    /// the new owner to accept the invitation and become a co-owner of the pet. The frontend URL is obtained from an
    /// environment variable. The encryption is performed using a security service, and the resulting encrypted string
    /// is appended to the frontend URL as a query parameter named "code". The method returns the generated ownership link
    /// or an error if the encryption fails.
    /// </summary>
    /// <param name="invitation"></param>
    /// <returns></returns>
    private Result<string> CreateOwnershipLink(OwnershipInvitation invitation)
    {
        // No tengo frontend, asi que ajá
        var baseLink = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:3000";
        var safeEmail = Uri.EscapeDataString(invitation.NewOwnerEmail);
        var safeName = Uri.EscapeDataString(invitation.NewOwnerName);
        return invitation.NewOwnerHasAccount
            ? $"{baseLink}/accept-invitation?code={invitation.InvitationCode}&email={safeEmail}"
            : $"{baseLink}/sign-up?invitationCode={invitation.InvitationCode}&email={safeEmail}&name={safeName}";
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sends an ownership invitation email to the new owner. It takes an ownership invitation and returns a result indicating
    /// whether the email was sent successfully or not. This method sends an ownership invitation email to the new owner
    /// using the notification service. It checks if the invitation is valid, and if it is expired, it deletes the expired
    /// invitation and creates a new one. Then, it sends the invitation email with the share link to the new owner's
    /// email address. The method returns a result indicating whether the email was sent successfully or if an error
    /// occurred during the process. If the invitation is invalid, it returns an error result indicating the reason for
    /// the invalidity. If the email is sent successfully, it returns a success result indicating that the ownership
    /// invitation was sent successfully. If an error occurs while sending the email, it returns an error result indicating
    /// that an error occurred while sending the notification email.
    /// </summary>
    /// <param name="invitation"></param>
    /// <returns></returns>
    private async Task<Result> SendOwnershipInvitation(OwnershipInvitation invitation)
    {
        // Validations
        if (invitation.Pet is null)
        {
            return new Result
            {
                Success = false,
                Code = "INVALID_PET",
                Status = 400,
                Title = "Invalid pet in the invitation.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }

        if (invitation.User is null)
        {
            return new Result
            {
                Success = false,
                Code = "INVALID_USER",
                Status = 400,
                Title = "Invalid user in the invitation.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }

        if (invitation.Expiration < DateTime.UtcNow)
        {
            // In case the invitation is expired, delete the expired one
            var deleteResult = await DeleteOwnershipInvitationAsync(invitation.Id);
            if (!deleteResult) return deleteResult;

            // Then create a new one
            var invitationResult = await CreateOwnershipInvitation(
                invitation.Pet,
                invitation.User,
                invitation.NewOwnerName,
                invitation.NewOwnerEmail
            );
            if (!invitationResult || invitationResult.Data is null) return invitationResult;

            // Update the old invitation
            var keepInvitation = invitation;
            invitation = invitationResult.Data;
            invitation.Pet = keepInvitation.Pet;
            invitation.User = keepInvitation.User;
        }

        // Create the link
        var linkResult = CreateOwnershipLink(invitation);
        if (!linkResult || linkResult.Data is null) return linkResult.ConvertTo<OwnershipInvitation>();
        invitation.OwnershipLink = linkResult.Data;

        // Send the invitation email
        var notificationResult = await _notificationService.SendOwnershipInvitationLink(invitation);
        return notificationResult
            ? new Result
            {
                Success = true,
                Code = "OWNERSHIP_INVITATION_SENT",
                Status = 200,
                Title = "Ownership invitation sent successfully.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            }
            : new Result
            {
                Success = false,
                Code = "ERROR_SENDING_NOTIFICATION",
                Status = 500,
                Title = "An error occurred while sending the notification email.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds a pet owner to a pet. It takes a pet and a user, and returns a result with the updated user or an error if
    /// something went wrong. This method adds a pet owner to a pet by creating a new UserPet relationship between the user
    /// and the pet.
    /// </summary>
    /// <param name="pet"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<Result<Pet?>> AddPetOwnerAsync(Pet pet, User user)
    {
        try
        {
            // Add the pet to the user
            var userPet = new UserPet
            {
                UserId = user.Id,
                PetId = pet.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.AddUserPet(userPet),
                operationName: "adding pet to user",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding pet to user");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_ADDING_PET_TO_USER",
                Status = 500,
                Title = "An error occurred while adding pet to user"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    private async Task<Result> HandleAcceptExistingRelationship(
        UserPet existingUserPet,
        OwnershipInvitation invitation,
        AcceptOwnershipInvitationRequest request
    )
    {
        // Validations
        if (existingUserPet.Status == EntityStatus.Active)
        {
            var deleteExistentInvitationResult = await DeleteOwnershipInvitationAsync(invitation.Id);
            if (!deleteExistentInvitationResult) return deleteExistentInvitationResult;

            return new Result
            {
                Success = false,
                Code = "USER_ALREADY_OWNER",
                Status = 400,
                Title = "The user is already a owner of the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }

        if (existingUserPet.Status == EntityStatus.Banned)
            return new Result
            {
                Success = false,
                Code = "USER_BANNED_FROM_PET",
                Status = 403,
                Title = "The user is banned from being owner of the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (existingUserPet.Status == EntityStatus.ToDelete)
            return new Result
            {
                Success = false,
                Code = "USER_PET_RELATIONSHIP_PENDING_DELETION",
                Status = 400,
                Title = "The user pet relationship is pending deletion, please try again later",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        // Update that existing one
        existingUserPet.Status = EntityStatus.Active;
        existingUserPet.UpdatedAt = DateTime.UtcNow;
        var updateResult = await _petRepo.UpdateUserPet(existingUserPet);
        if (!updateResult || updateResult.Data is null) return updateResult;

        var deleteInvitationResult = await DeleteOwnershipInvitationAsync(invitation.Id);
        if (!deleteInvitationResult) return deleteInvitationResult;
        _logger.LogInformation("Ownership invitation with code {InvitationCode} accepted successfully",
            request.InvitationCode);
        return new Result
        {
            Success = true,
            Code = "OWNERSHIP_INVITATION_ACCEPTED",
            Status = 200,
            Title = "Ownership invitation accepted successfully.",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    private async Task<Result<OwnershipInvitation?>> GetOwnershipInvitationByCodeAsync(string invitationCode,
        StatusFilters? filters = null)
    {
        if (string.IsNullOrEmpty(invitationCode))
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "INVALID_INVITATION_CODE",
                Status = 400,
                Title = "The invitation code is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        return await DbRetry.ExecuteWithRetry(
            operation: () => _petRepo.GetOwnershipInvitationByCodeAsync(invitationCode, filters),
            operationName: "getting ownership invitation",
            logger: _logger
        );
    }

    // -----------------------------------------------------------------------------------------------------------------
    private async Task<Result<OwnershipInvitation?>> AddOwnershipInvitationAsync(OwnershipInvitation invitation)
    {
        try
        {
            _logger.LogInformation("Adding ownership invitation for pet {PetId} to {Email}", invitation.PetId,
                invitation.NewOwnerEmail);

            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.AddOwnershipInvitationAsync(invitation),
                operationName: "Adding ownership invitation",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error creating ownership invitation");
            return new Result<OwnershipInvitation?>
            {
                Success = false,
                Code = "ERROR_CREATING_OWNERSHIP_INVITATION",
                Status = 500,
                Title = "An error occurred while creating the ownership invitation",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    private async Task<Result> DeleteOwnershipInvitationAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting ownership invitation with id {InvitationId}", id);

            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.DeleteOwnershipInvitationAsync(id),
                operationName: "Deleting ownership invitation",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error deleting ownership invitation");
            return new Result
            {
                Success = false,
                Code = "ERROR_DELETING_OWNERSHIP_INVITATION",
                Status = 500,
                Title = "An error occurred while deleting the ownership invitation",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    private async Task<Result<Pet?>> UpdatePetAsync(Pet pet)
    {
        try
        {
            _logger.LogInformation("Updating pet in repository with id {PetId}", pet.Id);

            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.UpdateAsync(pet),
                operationName: "Updating pet",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error updating pet");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_UPDATING_PET",
                Status = 500,
                Title = "An error occurred while updating the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the relationship between a user and a pet by their respective ids. This is useful to check if a user is the
    /// owner of a pet before allowing them to perform certain actions on the pet, such as editing its information or
    /// sharing its ownership with another user. It can also be used to get the details of the relationship, such as the
    /// status of the relationship (active, pending, etc.) and the dates of creation and last update of the relationship.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="petId"></param>
    /// <param name="filters"></param>
    /// <param name="includeUser">Whether to include the user in the result</param>
    /// <param name="includePet">Whether to include the pet in the result</param>
    /// <returns></returns>
    private async Task<Result<UserPet?>> GetUserPetByBothIdsAsync(
        int userId,
        int petId,
        StatusFilters? filters = null,
        bool includeUser = false,
        bool includePet = false
    )
    {
        try
        {
            _logger.LogInformation("Getting user pet relationship by user ID: {@UserId} and pet ID: {@PetId}", userId,
                petId);

            // Validations
            if (userId <= 0)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "INVALID_USER_ID",
                    Status = 400,
                    Title = "User id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (petId <= 0)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "INVALID_PET_ID",
                    Status = 400,
                    Title = "Pet id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.GetUserPetByBothIdsAsync(
                    userId,
                    petId,
                    filters,
                    includeUser,
                    includePet
                ),
                operationName: "Getting user pet by IDs",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting user pet by IDs");
            return new Result<UserPet?>
            {
                Success = false,
                Code = "ERROR_GETTING_USER_PET",
                Status = 500,
                Title = "An error occurred while getting the user pet relationship",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null
    )
    {
        try
        {
            _logger.LogInformation("Getting user by email");

            // Validations
            if (string.IsNullOrWhiteSpace(email))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_EMAIL",
                    Status = 400,
                    Title = "Email is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetByEmailAsync(
                    email,
                    filters
                ),
                operationName: "Getting user by email",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting user by email");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_GETTING_USER",
                Status = 500,
                Title = "An error occurred while getting the user",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a pet by its ID.
    /// </summary>
    /// <param name="id">The ID of the pet to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeUsers">Whether to include the user data in the query</param>
    /// <param name="includeOwnerInvitations">Whether to include the ownership invitations in the query</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public async Task<Result<Pet?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUsers = false,
        bool includeOwnerInvitations = false
    )
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
                    Title = "The pet ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var getResult = await DbRetry.ExecuteWithRetry(
                operation: () => _petRepo.GetByIdAsync(id, filters, includeUsers, includeOwnerInvitations),
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
                    Title = "Pet not found"
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
                Title = "An error occurred while getting the pet. Please try again later.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

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

            // Validations
            var requestResult = CheckCreatePetRequest(request);
            if (!requestResult) return requestResult.ConvertTo<Pet?>();

            // TODO check for existing with the same name and species pets for that user
            // Create the pet
            var pet = new Pet
            {
                Name = request.Name,
                Species = request.Species,
                Breed = request.Breed,
                Status = EntityStatus.Active,
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
                Title = "Pet created successfully",
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
                Title = "An error occurred while adding the pet. Please try again later."
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
                    Title = "The pet ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var updateResult = await UpdatePetAsync(pet);
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
                Title = "An error occurred while adding the pet",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<User?>> UpdateUserAsync(User user)
    {
        try
        {
            _logger.LogInformation("Updating user with id {UserId}", user.Id);

            if (user.Id <= 0)
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_USER_ID",
                    Status = 400,
                    Title = "The user ID is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var updateResult = await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.UpdateAsync(user),
                operationName: "Updating user",
                logger: _logger
            );
            _logger.LogInformation("User with id {UserId} updated successfully", user.Id);
            return updateResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error updating user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_UPDATING_USER",
                Status = 500,
                Title = "An error occurred while updating the user",
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
    /// <param name="invitationRequest"></param>
    /// <returns></returns>
    public async Task<Result> SendOwnershipInvitationAsync(SendOwnershipInvitationRequest invitationRequest)
    {
        try
        {
            _logger.LogInformation("Sharing ownership of pet {PetId} with user {Email}", invitationRequest.PetId,
                invitationRequest.NewOwnerEmail);

            // Validations
            var requestResult = CheckSharePetOwnershipRequest(invitationRequest);
            if (!requestResult) return requestResult;

            // Check for pet existence
            var filters = StatusFilters.ExcludeAll().ThenIncludeActive().ThenIncludeDeleted().ThenIncludeBanned();
            var existenceResult = await GetByIdAsync(
                invitationRequest.PetId,
                filters,
                true,
                true
            );
            if (!existenceResult || existenceResult.Data is null) return existenceResult;
            var pet = existenceResult.Data;

            // Pet has to be active to be able to accept
            // new owners
            if (pet.Status == EntityStatus.Banned)
                return new Result
                {
                    Success = false,
                    Code = "PET_BANNED",
                    Status = 403,
                    Title = "This pet is banned and cannot be used",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (pet.Status == EntityStatus.Deleted)
                return new Result
                {
                    Success = false,
                    Code = "PET_DELETED",
                    Status = 403,
                    Title = "This pet was deleted from this user and cannot be used",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check for the pet, it has to be
            // owned by this specific owner
            var userPet = pet.UserPets.FirstOrDefault(u => u.User?.Id == invitationRequest.UserId);
            if (userPet?.User is null)
                return new Result
                {
                    Success = false,
                    Code = "USER_NOT_OWNER",
                    Status = 403,
                    Title = "The user is not the owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (userPet.Status == EntityStatus.Banned)
                return new Result
                {
                    Success = false,
                    Code = "USER_BANNED_FROM_PET",
                    Status = 403,
                    Title = "The user is banned from being owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (userPet.Status == EntityStatus.Deleted)
                return new Result
                {
                    Success = false,
                    Code = "USER_DELETED_FROM_PET",
                    Status = 403,
                    Title = "The user is not longer the owner of this pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var owner = userPet.User;

            // You cannot send invitations if your ownership is invalid
            if (owner.Status != EntityStatus.Active)
                return new Result
                {
                    Success = false,
                    Code = "INVALID_OWNERSHIP",
                    Status = 403,
                    Title = "Your ownership of the pet is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // You can't send yourself an invitation
            if (owner.Email == invitationRequest.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_SHARE_WITH_SELF",
                    Status = 400,
                    Title = "You cannot share ownership with yourself",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // You can't also share with any existent co-owner
            var alreadyOwner = pet.UserPets.FirstOrDefault(u => u.User?.Email == invitationRequest.NewOwnerEmail);
            if (alreadyOwner is not null && alreadyOwner.Status == EntityStatus.Active)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_SHARE_WITH_EXISTING_OWNER",
                    Status = 400,
                    Title = "The user is already a co-owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check for existing invitations
            var existingInvitation = pet.OwnershipInvitations
                .FirstOrDefault(i => i.NewOwnerEmail == invitationRequest.NewOwnerEmail);
            if (existingInvitation is not null) return await SendOwnershipInvitation(existingInvitation);

            // Create a new invitation
            var createResult = await CreateOwnershipInvitation(
                pet,
                owner,
                invitationRequest.NewOwnerName,
                invitationRequest.NewOwnerEmail
            );
            if (!createResult || createResult.Data is null) return createResult;
            var invitation = createResult.Data;

            // here, Im pretty sure I wanna send that fcking amazing invitation
            return await SendOwnershipInvitation(invitation);
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error sharing pet ownership");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_SHARING_PET_OWNERSHIP",
                Status = 500,
                Title = "An error occurred while sharing pet ownership",
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
                Title = "The pet name is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.Name.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_NAME",
                Status = 400,
                Title = "The pet name must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.Breed.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_BREED",
                Status = 400,
                Title = "The pet breed must be less than 100 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (!Enum.IsDefined(typeof(PetSpecies), request.Species))
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_SPECIES",
                Status = 400,
                Title = "The pet species is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        return new Result
        {
            Success = true,
            Code = "SUCCESS",
            Status = 200,
            Title = "Pet added successfully",
            TraceCode = FileCodes.CallerIC(),
            Returnable = false
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Accepts an ownership invitation for a pet.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public async Task<Result> AcceptOwnershipInvitationAsync(
        AcceptOwnershipInvitationRequest request,
        StatusFilters? filters = null
    )
    {
        try
        {
            _logger.LogInformation("Accepting ownership invitation with code {InvitationCode}",
                request.InvitationCode);

            // Validations
            if (string.IsNullOrEmpty(request.InvitationCode))
                return new Result
                {
                    Success = false,
                    Code = "INVALID_INVITATION_CODE",
                    Status = 400,
                    Title = "The invitation code is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var getResult = await GetOwnershipInvitationByCodeAsync(request.InvitationCode, filters);
            if (!getResult || getResult.Data is null) return getResult;
            var invitation = getResult.Data;

            if (invitation.Expiration < DateTime.UtcNow)
                return new Result
                {
                    Success = false,
                    Code = "INVITATION_EXPIRED",
                    Status = 400,
                    Title = "The invitation has expired",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.Pet is null || invitation.User is null)
                return new Result
                {
                    Success = false,
                    Code = "INVALID_INVITATION",
                    Status = 400,
                    Title = "The invitation is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.NewOwnerEmail != request.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "EMAIL_MISMATCH",
                    Status = 400,
                    Title = "The email in the invitation does not match the email in the request",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.User.Email == request.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_ACCEPT_OWN_INVITATION",
                    Status = 400,
                    Title = "You cannot accept an invitation that you sent",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            //Get new owner objets
            var coOwnerResult = await _userRepo.GetByEmailAsync(invitation.NewOwnerEmail, filters);
            if (!coOwnerResult || coOwnerResult.Data is null)
                return new Result
                {
                    Success = false,
                    Code = "NEW_OWNER_NOT_FOUND",
                    Status = 404,
                    Title = "The new owner user was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var coOwner = coOwnerResult.Data;

            // Avoid adding existent UserPet relationship
            filters = StatusFilters.IncludeAll();
            var existingOwnerResult = await _petRepo.GetUserPetByBothIdsAsync(coOwner.Id, invitation.Pet.Id, filters);

            // There is another relationship
            if (existingOwnerResult && existingOwnerResult.Data is not null)
                return await HandleAcceptExistingRelationship(existingOwnerResult.Data, invitation, request);

            // There is no existent relationship
            var addOwnerResult = await AddPetOwnerAsync(invitation.Pet, coOwner);
            if (!addOwnerResult) return addOwnerResult;

            var deleteInvitationResult = await DeleteOwnershipInvitationAsync(invitation.Id);
            if (!deleteInvitationResult) return deleteInvitationResult;

            _logger.LogInformation("Ownership invitation with code {InvitationCode} accepted successfully",
                request.InvitationCode);
            return new Result
            {
                Success = true,
                Code = "OWNERSHIP_INVITATION_ACCEPTED",
                Status = 200,
                Title = "Ownership invitation accepted successfully.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error accepting pet ownership");
            return new Result<Pet?>
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
    /// <summary>
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<Pet?>> AddNewPetAsync(AddNewPetRequest request)
    {
        try
        {
            _logger.LogInformation("Adding pet with name {@Name} to user with id: {@Id}", request.Pet.Name,
                request.UserId);

            // Validations
            if (request.UserId <= 0)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "INVALID_USER_ID",
                    Status = 400,
                    Title = "User id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var petCheckResult = CheckCreatePetRequest(request.Pet);
            if (!petCheckResult) return petCheckResult.ConvertTo<Pet?>();

            // Find the user to add the pet to
            var userResult = await _userRepo.GetByIdAsync(request.UserId);
            if (userResult.Data is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Title = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userResult.Data;

            // Check if the user is verified
            if (user.Status == EntityStatus.Banned)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 400,
                    Title = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status == EntityStatus.Unverified)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_NOT_VERIFIED",
                    Status = 400,
                    Title = "User is not verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Create the pet
            var petResult = await CreatePetAsync(request.Pet);
            if (!petResult || petResult.Data is null) return petResult.ConvertTo<Pet?>();
            var pet = petResult.Data;

            // Add the pet to the user
            var userPet = new UserPet
            {
                UserId = request.UserId,
                PetId = pet.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            var addResult = await _petRepo.AddUserPet(userPet);

            return addResult && addResult.Data is not null
                ? new Result<Pet?>
                {
                    Success = true,
                    Code = "PET_ADDED_TO_USER",
                    Status = 200,
                    Title = "Pet added to user successfully",
                    Data = addResult.Data,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : new Result<Pet?>
                {
                    Success = false,
                    Code = "ERROR_ADDING_PET_TO_USER",
                    Status = 500,
                    Title = "An error occurred while adding pet to user",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding pet to user");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_ADDING_PET_TO_USER",
                Status = 500,
                Title = "An error occurred while adding pet to user"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Removes a pet from the user. It takes the device id from the header and the remove pet from user request from
    /// the body. It checks if the user is verified before removing the pet from the user. If the user is not verified,
    /// it returns a bad request with a message indicating that the user is not verified. It also checks if the user is
    /// the owner of the pet before allowing them to remove the pet from their account. If the user is not the owner of
    /// the pet, it returns a bad request with a message indicating that the user is not the owner of the pet. Finally,
    /// it doesn't delete the relationship between the user and the pet, but instead it sets its status to deleted, so
    /// the information is not lost and can be used for analytics and other purposes in the future.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<Pet?>> RemovePetAsync(RemovePetRequest request)
    {
        try
        {
            _logger.LogInformation("Removing pet with id {@PetId} from user with id: {@Id}", request.PetId,
                request.UserId);

            // Get thet user pet relationship
            var userPetResult = await GetUserPetByBothIdsAsync(
                request.UserId,
                request.PetId,
                null,
                true,
                true
            );

            if (!userPetResult || userPetResult.Data is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_PET_RELATIONSHIP_NOT_FOUND",
                    Status = 404,
                    Title = "The relationship between the user and the pet was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var userPet = userPetResult.Data;

            // Check user status
            if (userPet.User is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Title = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userPet.User;

            if (user.Status == EntityStatus.Banned)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 400,
                    Title = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status == EntityStatus.Unverified)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "USER_NOT_VERIFIED",
                    Status = 400,
                    Title = "User is not verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check pet status
            if (userPet.Pet is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "PET_NOT_FOUND",
                    Status = 404,
                    Title = "Pet not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var pet = userPet.Pet;

            if (pet.Status == EntityStatus.Banned)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "PET_BANNED",
                    Status = 400,
                    Title = "Pet is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (pet.Status == EntityStatus.Deleted)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "PET_DELETED",
                    Status = 400,
                    Title = "Pet was already deleted",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // TODO add tokens so only the logged in user can do those things
            // Remove the pet from the user by setting the relationship status to deleted
            userPet.Status = EntityStatus.Deleted;
            userPet.UpdatedAt = DateTime.UtcNow;
            var filters = StatusFilters.ExcludeAll().ThenIncludeDeleted();
            var deleteResult = await _petRepo.UpdateUserPet(userPet, filters);
            if (!deleteResult || deleteResult.Data is null)
                return new Result<Pet?>
                {
                    Success = false,
                    Code = "ERROR_DELETING_USER_PET",
                    Status = 500,
                    Title = "An error occurred while deleting user pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            return deleteResult.Data.Status == EntityStatus.Deleted
                ? new Result<Pet?>
                {
                    Success = true,
                    Code = "PET_REMOVED_FROM_USER",
                    Status = 200,
                    Title = "Pet removed from user successfully",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : new Result<Pet?>
                {
                    Success = false,
                    Code = "ERROR_REMOVING_PET_FROM_USER",
                    Status = 500,
                    Title = "An error occurred while removing pet from user",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error removing pet from user");
            return new Result<Pet?>
            {
                Success = false,
                Code = "ERROR_REMOVING_PET_FROM_USER",
                Status = 500,
                Title = "An error occurred while removing pet from user",
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

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets all pets by owner. It takes the device id from the header and returns a list of pets owned by the user. It
    /// also checks if the user is verified before returning the pets. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<User?>> GetPetsByOwnerAsync(
        string deviceId,
        string sessionToken,
        GetPetsByOwnerRequest request
    )
    {
        try
        {
            // Get the user
            var userResult = await _userRepo.GetByIdAsync(
                request.OwnerId,
                StatusFilters.ExcludeAll().ThenIncludeActive(),
                true,
                true
            );
            if (!userResult || userResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Title = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userResult.Data;

            // Check Token
            if (user.SessionToken is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "SESSION_TOKEN_NOT_FOUND",
                    Status = 401,
                    Title = "Session token not found"
                };

            // TODO Implement Auth service as a new layer
            var tokenResult = SecurityService.IsSessionTokenValid(deviceId, user.SessionToken);
            if (!tokenResult.Success)
            {
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_SESSION_TOKEN",
                    Status = 401,
                    Title = "Invalid session token"
                };
            }

            // Renew token
            user.SessionToken.Renew();
            var updateResult = await UpdateUserAsync(user);
            if (!updateResult) return updateResult.ConvertTo<User?>();

            // Check for pets
            return user.UserPets.Count > 0
                ? new Result<User?>
                {
                    Success = true,
                    Code = "PETS_FOUND",
                    Status = 200,
                    Title = "Pets found for this user",
                    Data = user,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : new Result<User?>
                {
                    Success = true,
                    Code = "NO_PETS_FOUND",
                    Status = 200,
                    Title = "No pets found for this user",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error getting pets by owner with device id {deviceId}");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_GETTING_PETS_BY_OWNER",
                Status = 500,
                Title = "An error occurred while getting the pets by owner. Please try again later.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}