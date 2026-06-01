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
using Newtonsoft.Json;

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
    private static Result CheckSharePetOwnershipRequest(SendOwnershipInvitationRequest invitationRequest)
    {
        if (invitationRequest.UserId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_USER_ID",
                Status = 400,
                Message = "The user ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (invitationRequest.PetId <= 0)
            return new Result
            {
                Success = false,
                Code = "INVALID_PET_ID",
                Status = 400,
                Message = "The pet ID is invalid",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (string.IsNullOrEmpty(invitationRequest.NewOwnerEmail))
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "The email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (invitationRequest.NewOwnerEmail.Length > 100)
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "The email must be less than 100 characters",
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
        var newOwnerResult = await _userRepo.GetByEmailAsync(newOwnerEmail);

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
                InvitationCode = Guid.NewGuid().ToString()
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
                InvitationCode = Guid.NewGuid().ToString()
            };

        // Save and return this invitation
        var addResult = await _petRepo.AddOwnershipInvitationAsync(invitation);
        if (!addResult || addResult.Data is null)
            return new Result<OwnershipInvitation>
            {
                Success = false,
                Code = "ERROR_CREATING_OWNERSHIP_INVITATION",
                Status = 500,
                Message = "An error occurred while creating the ownership invitation",
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
                Message = "Invalid pet in the invitation.",
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
                Message = "Invalid user in the invitation.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }

        if (invitation.Expiration < DateTime.UtcNow)
        {
            // In case the invitation is expired, delete the expired one
            var deleteResult = await _petRepo.DeleteOwnershipInvitationAsync(invitation.Id);
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
                Message = "Ownership invitation sent successfully.",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            }
            : new Result
            {
                Success = false,
                Code = "ERROR_SENDING_NOTIFICATION",
                Status = 500,
                Message = "An error occurred while sending the notification email.",
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
    private async Task<Result<User?>> AddPetOwnerAsync(Pet pet, User user)
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
                Status = GenericStatus.Active
            };

            return await _userRepo.AddUserPet(userPet);
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding pet to user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_ADDING_PET_TO_USER",
                Status = 500,
                Message = "An error occurred while adding pet to user"
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
                Status = GenericStatus.Active,
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
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public async Task<Result<Pet?>> GetByIdAsync(int id, StatusFilters? filters = null)
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
                operation: () => _petRepo.GetByIdAsync(id, filters),
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
    /// <param name="invitationRequest"></param>
    /// <returns></returns>
    public async Task<Result> SendOwnershipInvitationAsync(SendOwnershipInvitationRequest invitationRequest)
    {
        try
        {
            _logger.LogInformation("Sharing ownership of pet {PetId} with user {Email}", invitationRequest.PetId,
                invitationRequest.NewOwnerEmail);

            // Verifications
            var requestResult = CheckSharePetOwnershipRequest(invitationRequest);
            if (!requestResult) return requestResult;

            // Check for pet existence
            var existenceResult = await GetByIdAsync(invitationRequest.PetId);
            if (!existenceResult || existenceResult.Data is null) return existenceResult;
            var pet = existenceResult.Data;

            // Check for the pet, it has to be
            // owned by this specific owner
            var userPet = pet.UserPets.FirstOrDefault(u => u.User?.Id == invitationRequest.UserId);
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

            // You can't send yourself an invitation
            if (owner.Email == invitationRequest.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_SHARE_WITH_SELF",
                    Status = 400,
                    Message = "You cannot share ownership with yourself",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // You can't also share with any existent co-owner
            var alreadyOwner = pet.UserPets.Any(u => u.User?.Email == invitationRequest.NewOwnerEmail);
            if (alreadyOwner)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_SHARE_WITH_EXISTING_OWNER",
                    Status = 400,
                    Message = "The user is already a co-owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check for existing invitations
            var existingInvitation =
                pet.OwnershipInvitations.FirstOrDefault(i => i.NewOwnerEmail == invitationRequest.NewOwnerEmail);
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
                    Message = "The invitation code is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var getResult = await _petRepo.GetOwnershipInvitationByCodeAsync(request.InvitationCode, filters);
            if (!getResult || getResult.Data is null) return getResult;
            var invitation = getResult.Data;

            if (invitation.Expiration < DateTime.UtcNow)
                return new Result
                {
                    Success = false,
                    Code = "INVITATION_EXPIRED",
                    Status = 400,
                    Message = "The invitation has expired",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.Pet is null || invitation.User is null)
                return new Result
                {
                    Success = false,
                    Code = "INVALID_INVITATION",
                    Status = 400,
                    Message = "The invitation is invalid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.NewOwnerEmail != request.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "EMAIL_MISMATCH",
                    Status = 400,
                    Message = "The email in the invitation does not match the email in the request",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (invitation.User.Email == request.NewOwnerEmail)
                return new Result
                {
                    Success = false,
                    Code = "CANNOT_ACCEPT_OWN_INVITATION",
                    Status = 400,
                    Message = "You cannot accept an invitation that you sent",
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
                    Message = "The new owner user was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var coOwner = coOwnerResult.Data;

            // Avoid adding existent UserPet relationship
            var existingOwnerResult = await _userRepo.GetUserPetByBothIdsAsync(coOwner.Id, invitation.Pet.Id);
            if (existingOwnerResult)
                return new Result
                {
                    Success = false,
                    Code = "USER_PET_RELATIONSHIP_EXISTS",
                    Status = 400,
                    Message = "The user is already a owner of the pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Here, I will add the user as a co-owner of the pet and delete the invitation
            var addOwnerResult = await AddPetOwnerAsync(invitation.Pet, coOwner);
            if (!addOwnerResult) return addOwnerResult;

            var deleteInvitationResult = await _petRepo.DeleteOwnershipInvitationAsync(invitation.Id);
            if (!deleteInvitationResult) return deleteInvitationResult;

            _logger.LogInformation("Ownership invitation with code {InvitationCode} accepted successfully",
                request.InvitationCode);
            return new Result
            {
                Success = true,
                Code = "OWNERSHIP_INVITATION_ACCEPTED",
                Status = 200,
                Message = "Ownership invitation accepted successfully.",
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
                Message = "An error occurred while accepting pet ownership",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}