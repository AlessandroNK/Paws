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

/// <summary>
/// Provides functionality for managing user operations.
/// </summary>
/// <remarks>FU02</remarks>
public class UserService(
    INotificationService notificationService,
    IUserRepository userRepo,
    IPetService petService,
    ILogger<UserService> logger
) : IUserService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The user repository used to access and manage users in the application. It is injected into the service to provide
    /// access to the underlying data source for user-related operations, such as creating, updating, deleting, and
    /// retrieving users. The service uses the repository to perform the necessary data access logic for user management.
    /// </summary>
    private readonly IUserRepository _userRepo = userRepo;

    /// <summary>
    /// The pets service used to manage pets in the application. It is injected into the service to provide access to the
    /// underlying pet management functionality for operations that involve both users and pets, such as adding a pet to
    /// a user. The service uses the pets service to perform the necessary logic for managing pets in relation to users,
    /// such as creating, updating, deleting, and retrieving pets associated with users. This allows for a separation of
    /// concerns and promotes modularity in the application, as the user service can focus on user-related operations
    /// while delegating pet-related operations to the pets service.
    /// </summary>
    private readonly IPetService _petService = petService;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<UserService> _logger = logger;

    /// <summary>
    /// The service to notify users. It is injected into the service to provide access to the underlying notification
    /// system for sending notifications to users. The service uses the notification service to send relevant notifications
    /// to users, such as verification codes, password reset instructions, and other important updates related to their
    /// accounts. Those notifications can be sent by frontend, email or sms.
    /// </summary>
    private readonly INotificationService _notificationService = notificationService;


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
    /// Adds a new user to the db
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<Result<User?>> AddAsync(User user)
    {
        try
        {
            _logger.LogInformation("Adding new user");

            var addResult = await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.AddAsync(user),
                operationName: "Adding new user",
                logger: _logger
            );

            return addResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error adding new user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_ADDING_USER",
                Status = 500,
                Message = "An error occurred while adding the user",
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
    /// <returns></returns>
    private async Task<Result<UserPet?>> GetUserPetByBothIdsAsync(int userId, int petId, StatusFilters? filters = null)
    {
        try
        {
            _logger.LogInformation("Getting user pet relationship by user ID: {@UserId} and pet ID: {@PetId}", userId, petId);

            // Validations
            if (userId <= 0)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "INVALID_USER_ID",
                    Status = 400,
                    Message = "User id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (petId <= 0)
                return new Result<UserPet?>
                {
                    Success = false,
                    Code = "INVALID_PET_ID",
                    Status = 400,
                    Message = "Pet id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetUserPetByBothIdsAsync(
                    userId,
                    petId,
                    filters
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
                Message = "An error occurred while getting the user pet relationship",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    #region Helpers

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a sign up request
    /// </summary>
    /// <param name="request">The sign up request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    private static Result CheckSignUpRequest(SignUpRequest request)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(request.Email))
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "Email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        var passwordResult = SecurityService.IsPasswordValid(request.Password);
        if (!passwordResult) return passwordResult;

        if (!Enum.IsDefined(typeof(DocumentType), request.DocumentType))
            return new Result
            {
                Success = false,
                Code = "INVALID_DOCUMENT_TYPE",
                Status = 400,
                Message = "Document type must be between 1 and 5",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.DocumentNumber is { Length: < 1 or > 15 })
            return new Result
            {
                Success = false,
                Code = "INVALID_DOCUMENT_NUMBER",
                Status = 400,
                Message = "Document number must be between 1 and 15 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (
            request.Name is { Length: < 1 or > 200 } ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.Name, @"^[\p{L}\p{M}\s'\-]+$")
        )
            return new Result
            {
                Success = false,
                Code = "INVALID_NAME",
                Status = 400,
                Message =
                    "The name must be between 1 and 200 characters and can only contain letters, spaces, hyphens, and apostrophes",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        var emailResult = SecurityService.ValidateEmailAddress(request.Email);
        if (!emailResult) return emailResult;

        // Everything is valid
        return new Result
        {
            Success = true,
            Code = "SUCCESS_REQUEST",
            Status = 200,
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    private Result AccountVerificationRequestValidations(AccountVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new Result
            {
                Success = false,
                Code = "INVALID_EMAIL",
                Status = 400,
                Message = "Email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.VerificationCode.Contains(" ") || request.VerificationCode.Length != 6)
            return new Result
            {
                Success = false,
                Code = "INVALID_VERIFICATION_CODE",
                Status = 400,
                Message = "Verification code must be 6 characters and cannot contain spaces",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        return new Result
        {
            Success = true,
            Code = "SUCCESS_REQUEST",
            Status = 200,
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }

    #endregion

    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------

    #region UserRelated

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
                    Message = "Email is required",
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
                Message = "An error occurred while getting the user",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its document number.
    /// </summary>
    /// <param name="document">The document to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByDocumentAsync(
        string document,
        StatusFilters? filters = null
    )
    {
        try
        {
            _logger.LogInformation("Getting user by document");

            // Validations
            if (string.IsNullOrWhiteSpace(document))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_DOCUMENT",
                    Status = 400,
                    Message = "Document is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetByDocumentAsync(document, filters),
                operationName: "Getting user by document",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting user by document");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_GETTING_USER",
                Status = 500,
                Message = "An error occurred while getting the user",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds a user by its ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null
    )
    {
        try
        {
            _logger.LogInformation("Getting user by ID");

            // Validations
            if (id <= 0)
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_ID",
                    Status = 400,
                    Message = "ID is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetByIdAsync(
                    id,
                    filters
                ),
                operationName: "Getting user by ID",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting user by ID");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_GETTING_USER",
                Status = 500,
                Message = "An error occurred while getting the user",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public async Task<Result<User?>> SignUp(SignUpRequest request)
    {
        try
        {
            _logger.LogInformation("Signing up user");

            // Validations
            var requestResult = CheckSignUpRequest(request);
            if (!requestResult) return requestResult.ConvertTo<User?>();

            // Find the user to avoid duplications
            var filters = StatusFilters.IncludeAll();
            var existingEmailResult = await GetByEmailAsync(request.Email, filters);
            var existingDocumentResult = await GetByDocumentAsync(request.DocumentNumber, filters);

            if (existingEmailResult.Data is not null || existingDocumentResult.Data is not null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_ALREADY_EXISTS",
                    Status = 400,
                    Message = "A user with the same email or document already exists",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Create the new user
            var user = new User
            {
                Password = request.Password,
                Email = request.Email,
                DocumentType = (DocumentType)request.DocumentType,
                DocumentNumber = request.DocumentNumber,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = EntityStatus.Unverified,
                VerificationCode = SecurityService.GenerateVerificationCode(),
                VerificationCodeDate = DateTime.UtcNow
            };

            // Save the user
            var addResult = await AddAsync(user);
            if (!addResult || addResult.Data is null) return addResult;
            user = addResult.Data;

            // Send verification code after signing up the user
            var notificationResult = await _notificationService.SendVerificationCodeAsync(
                user.Name,
                user.Email,
                user.VerificationCode
            );
            if (!notificationResult) return notificationResult.ConvertTo<User?>();

            // Everything went well, check for pet co-ownership
            if (string.IsNullOrWhiteSpace(request.InvitationCode))
                return new Result<User?>
                {
                    Success = true,
                    Code = "VERIFICATION_CODE_SENT",
                    Status = 201,
                    Message = "User signed up successfully",
                    Data = user,
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Process pet ownership request
            var invitationRequest = new AcceptOwnershipInvitationRequest
            {
                InvitationCode = request.InvitationCode,
                NewOwnerEmail = user.Email
            };
            filters = StatusFilters.Create().ThenIncludeUnverified();
            var ownershipResult = await _petService.AcceptOwnershipInvitationAsync(invitationRequest, filters);
            return ownershipResult
            ?  new Result<User?>
            {
                Success = true,
                Code = "VERIFICATION_CODE_SENT",
                Status = 201,
                Message = "User signed up successfully",
                Data = user,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            }
            : ownershipResult.ConvertTo<User?>();
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error signing up user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_SIGNING_UP_USER",
                Status = 500,
                Message = "An error occurred while signing up the user"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Verifies the account by checking if the provided code matches the database's verification code for the specified
    /// user.
    /// </summary>
    /// <param name="request">The <see cref="AccountVerificationRequest"/> containing the email and the verification
    /// code</param>
    /// <returns></returns>
    public async Task<Result<User?>> VerifyAccountAsync(AccountVerificationRequest request)
    {
        try
        {
            _logger.LogInformation("Verifying account for user with email: {@Email}", request.Email);

            // Validations
            var verificationResult = AccountVerificationRequestValidations(request);
            if (!verificationResult.Success) return verificationResult.ConvertTo<User?>();

            // Find the user to get the code
            var filters = StatusFilters.Create().ThenIncludeUnverified();
            var existingEmailResult = await GetByEmailAsync(request.Email, filters);
            if (existingEmailResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Message = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = existingEmailResult.Data;

            // Check if the user needs to be verified
            if (user.Status == EntityStatus.Banned)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 403,
                    Message = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status != EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_ALREADY_VERIFIED",
                    Status = 400,
                    Message = "User is already verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check if the code is correct
            if (user.VerificationCode != request.VerificationCode)
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_VERIFICATION_CODE",
                    Status = 400,
                    Message = "Invalid verification code",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Update the user
            user.VerificationCode = string.Empty;
            user.Status = EntityStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userRepo.UpdateAsync(user);

            updateResult.Code = !updateResult || updateResult.Data is null
                ? "ERROR_VERIFYING_ACCOUNT"
                : "ACCOUNT_VERIFIED";

            return updateResult;
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error signing up user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_VERIFYING_ACCOUNT",
                Status = 500,
                Message = "An error occurred while verifying the account"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resends the verification code to the user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<User?>> ResendVerificationEmailAsync(ResendVerificationCodeRequest request)
    {
        try
        {
            _logger.LogInformation("Resending verification email for user with email: {@Email}", request.Email);

            // Validations
            if (string.IsNullOrWhiteSpace(request.Email))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_EMAIL",
                    Status = 400,
                    Message = "Email is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Find the user to get the code
            var filters = StatusFilters.Create()
                .ThenIncludeUnverified()
                .ThenIncludeBanned()
                .ThenIncludeInactive()
                .ThenIncludeArchived();
            var existingResult = await GetByEmailAsync(request.Email, filters);
            if (existingResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Message = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = existingResult.Data;

            // Check if the user needs to be verified
            if (user.Status == EntityStatus.Banned)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 403,
                    Message = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status != EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_ALREADY_VERIFIED",
                    Status = 400,
                    Message = "User is already verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Verify if at least 1 minute has passed since the last code send ask
            // to avoid flooding my API because I have to pay for those
            // misused resources
            if (user.VerificationCodeDate.AddMinutes(1) > DateTime.UtcNow)
                return new Result<User?>
                {
                    Success = false,
                    Code = "TOO_MANY_REQUESTS",
                    Status = 429,
                    Message = "You can only request a new verification code every 1 minute",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Verify if the code is expired or not
            if (user.VerificationCodeDate.AddMinutes(15) < DateTime.UtcNow)
            {
                user.VerificationCode = SecurityService.GenerateVerificationCode();
                user.VerificationCodeDate = DateTime.UtcNow;
                filters = StatusFilters.IncludeAll().ThenExcludeDeleted();
                var updateResult = await _userRepo.UpdateAsync(user, filters);
                if (!updateResult || updateResult.Data is null)
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "ERROR_UPDATING_VERIFICATION_CODE",
                        Status = 500,
                        Message = "An error occurred while updating the verification code",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                user = updateResult.Data;
            }

            // Send the code
            var notificationResult = await _notificationService.SendVerificationCodeAsync(
                user.Name,
                user.Email,
                user.VerificationCode
            );
            if (!notificationResult) return notificationResult.ConvertTo<User?>();

            // Everything went well, return the user
            return new Result<User?>
            {
                Success = true,
                Code = "VERIFICATION_CODE_SENT",
                Status = 201,
                Message = "User signed up successfully",
                Data = user,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error signing up user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_VERIFYING_ACCOUNT",
                Status = 500,
                Message = "An error occurred while verifying the account"
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<User?>> AddNewPetAsync(AddNewPetRequest request)
    {
        try
        {
            _logger.LogInformation("Adding pet with name {@Name} to user with id: {@Id}", request.Pet.Name,
                request.UserId);

            // Validations
            if (request.UserId <= 0)
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_USER_ID",
                    Status = 400,
                    Message = "User id must be greater than 0",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            var petCheckResult = _petService.CheckCreatePetRequest(request.Pet);
            if (!petCheckResult) return petCheckResult.ConvertTo<User?>();

            // Find the user to add the pet to
            var userResult = await _userRepo.GetByIdAsync(request.UserId);
            if (userResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Message = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userResult.Data;

            // Check if the user is verified
            if (user.Status == EntityStatus.Banned)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 400,
                    Message = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status == EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_VERIFIED",
                    Status = 400,
                    Message = "User is not verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Create the pet
            var petResult = await _petService.CreatePetAsync(request.Pet);
            if (!petResult || petResult.Data is null) return petResult.ConvertTo<User?>();
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
    public async Task<Result<User?>> RemovePetAsync(RemovePetRequest request)
    {
        try
        {
            _logger.LogInformation("Removing pet with id {@PetId} from user with id: {@Id}", request.PetId,
                request.UserId);

            // Get thet user pet relationship
            var userPetResult = await GetUserPetByBothIdsAsync(request.UserId, request.PetId);
            if (!userPetResult || userPetResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_PET_RELATIONSHIP_NOT_FOUND",
                    Status = 404,
                    Message = "The relationship between the user and the pet was not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var userPet = userPetResult.Data;

            // Check user status
            if (userPet.User is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_FOUND",
                    Status = 404,
                    Message = "User not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var user = userPet.User;

            if (user.Status == EntityStatus.Banned)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_BANNED",
                    Status = 400,
                    Message = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if  (user.Status == EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_NOT_VERIFIED",
                    Status = 400,
                    Message = "User is not verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Check pet status
            if (userPet.Pet is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "PET_NOT_FOUND",
                    Status = 404,
                    Message = "Pet not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var pet = userPet.Pet;

            if (pet.Status == EntityStatus.Banned)
                return new Result<User?>
                {
                    Success = false,
                    Code = "PET_BANNED",
                    Status = 400,
                    Message = "Pet is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (pet.Status == EntityStatus.Deleted)
                return new Result<User?>
                {
                    Success = false,
                    Code = "PET_DELETED",
                    Status = 400,
                    Message = "Pet was already deleted",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // TODO add tokens so only the logged in user can do those things
            // Remove the pet from the user by setting the relationship status to deleted
            userPet.Status = EntityStatus.Deleted;
            userPet.UpdatedAt = DateTime.UtcNow;
            var filters = StatusFilters.ExcludeAll().ThenIncludeDeleted();
            var deleteResult = await _userRepo.UpdateUserPet(userPet, filters);
            if (!deleteResult || deleteResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "ERROR_DELETING_USER_PET",
                    Status = 500,
                    Message = "An error occurred while deleting user pet",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            return deleteResult.Data.Status == EntityStatus.Deleted
                ? new Result<User?>
                {
                    Success = true,
                    Code = "PET_REMOVED_FROM_USER",
                    Status = 200,
                    Message = "Pet removed from user successfully",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                }
                : new Result<User?>
                {
                    Success = false,
                    Code = "ERROR_REMOVING_PET_FROM_USER",
                    Status = 500,
                    Message = "An error occurred while removing pet from user",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error removing pet from user");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_REMOVING_PET_FROM_USER",
                Status = 500,
                Message = "An error occurred while removing pet from user",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    #endregion
}