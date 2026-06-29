using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
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
                Title = "An error occurred while adding the user",
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
                Title = "Email is required",
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
                Title = "Document type must be between 1 and 5",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.DocumentNumber is { Length: < 1 or > 15 })
            return new Result
            {
                Success = false,
                Code = "INVALID_DOCUMENT_NUMBER",
                Status = 400,
                Title = "Document number must be between 1 and 15 characters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (
            request.Name is { Length: < 1 or > 200 } ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.Name, @"^[\p{L}\p{M}\s'.\-]+$")
        )
            return new Result
            {
                Success = false,
                Code = "INVALID_NAME",
                Status = 400,
                Title =
                    "The name must be between 1 and 200 characters and can only contain letters, spaces, hyphens, apostrophes, and dots",
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
                Title = "Email is required",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };

        if (request.VerificationCode.Contains(" ") || request.VerificationCode.Length != 6)
            return new Result
            {
                Success = false,
                Code = "INVALID_VERIFICATION_CODE",
                Status = 400,
                Title = "Verification code must be 6 characters and cannot contain spaces",
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
    /// <param name="includeSessionToken">Whether to include the session token in the result</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        StatusFilters? filters = null,
        bool includeSessionToken = false
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
                    filters,
                    includeSessionToken
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
                    Title = "Document is required",
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
                Title = "An error occurred while getting the user",
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
        StatusFilters? filters = null,
        bool includePets = false
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
                    Title = "ID is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the user
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetByIdAsync(
                    id,
                    filters,
                    includePets
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
                Title = "An error occurred while getting the user",
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
                    Title = "A user with the same email or document already exists",
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
                    Title = "User signed up successfully",
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
                ? new Result<User?>
                {
                    Success = true,
                    Code = "VERIFICATION_CODE_SENT",
                    Status = 201,
                    Title = "User signed up successfully",
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
                Title = "An error occurred while signing up the user"
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
                    Title = "User not found",
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
                    Title = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status != EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_ALREADY_VERIFIED",
                    Status = 400,
                    Title = "User is already verified",
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
                    Title = "Invalid verification code",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Update the user
            user.VerificationCode = string.Empty;
            user.VerificationCodeDate = null;
            user.Status = EntityStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userRepo.UpdateAsync(user);
            updateResult.Log(_logger, "Error updating user status to active after verification");

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
                Title = "An error occurred while verifying the account"
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
                    Title = "Email is required",
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
                    Title = "User not found",
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
                    Title = "User is banned",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (user.Status != EntityStatus.Unverified)
                return new Result<User?>
                {
                    Success = false,
                    Code = "USER_ALREADY_VERIFIED",
                    Status = 400,
                    Title = "User is already verified",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            user.VerificationCodeDate ??= DateTime.UtcNow.AddMinutes(-16);

            // Verify if at least 1 minute has passed since the last code send ask
            // to avoid flooding my API because I have to pay for those
            // misused resources
            if (user.VerificationCodeDate?.AddMinutes(1) > DateTime.UtcNow)
                return new Result<User?>
                {
                    Success = false,
                    Code = "TOO_MANY_REQUESTS",
                    Status = 429,
                    Title = "You can only request a new verification code every 1 minute",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Verify if the code is expired or not
            if (user.VerificationCodeDate?.AddMinutes(15) < DateTime.UtcNow)
            {
                user.VerificationCode = SecurityService.GenerateVerificationCode();
                user.VerificationCodeDate = DateTime.UtcNow;
                filters = StatusFilters.IncludeAll().ThenExcludeDeleted();
                var updateResult = await _userRepo.UpdateAsync(user, filters);
                updateResult.Log(_logger, $"Error updating verification code for user with email: {user.Email}");

                if (!updateResult || updateResult.Data is null)
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "ERROR_UPDATING_VERIFICATION_CODE",
                        Status = 500,
                        Title = "An error occurred while updating the verification code",
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
                Title = "User verification code sent successfully",
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
                Title = "An error occurred while verifying the account",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<SessionToken?>> GetSessionTokenByTokenAsync(
        string requestTokenHash,
        StatusFilters? filters = null,
        bool includeUser = false
    )
    {
        try
        {
            _logger.LogInformation("Getting session token by token");

            // Validations
            if (string.IsNullOrWhiteSpace(requestTokenHash))
                return new Result<SessionToken?>
                {
                    Success = false,
                    Code = "INVALID_TOKEN",
                    Status = 400,
                    Title = "Token is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Search for the session token
            return await DbRetry.ExecuteWithRetry(
                operation: () => _userRepo.GetSessionTokenByTokenAsync(requestTokenHash, filters, includeUser),
                operationName: "Getting session token by token",
                logger: _logger
            );
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error getting session token by token");
            return new Result<SessionToken?>
            {
                Success = false,
                Code = "ERROR_GETTING_SESSION_TOKEN",
                Status = 500,
                Title = "An error occurred while getting the session token",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<User?>> ValidateSessionTokenAsync(string deviceId, string requestTokenHash)
    {
        try
        {
            _logger.LogInformation("Validating session token");

            // Validations
            if (string.IsNullOrWhiteSpace(requestTokenHash))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_TOKEN",
                    Status = 400,
                    Title = "Token is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Find the token
            var tokenResult = await GetSessionTokenByTokenAsync(requestTokenHash, StatusFilters.IncludeAll(), true);

            if (tokenResult.Data is null)
                return new Result<User?>
                {
                    Success = false,
                    Code = "TOKEN_NOT_FOUND",
                    Status = 404,
                    Title = "Token not found",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            var token = tokenResult.Data;

            // Validate Token
            var tokenValidationResult = SecurityService.IsSessionTokenValid(deviceId, token);
            if (!tokenValidationResult) return tokenValidationResult.ConvertTo<User?>();

            // Get user back
            var userResult = await GetByIdAsync(token.UserId, StatusFilters.IncludeAll(), true);
            if (!userResult || userResult.Data is null) return userResult.ConvertTo<User?>();
            var user = userResult.Data;

            switch (user.Status)
            {
                case EntityStatus.Unverified:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_UNVERIFIED",
                        Status = 403,
                        Title = "User is unverified",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Banned:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_BANNED",
                        Status = 403,
                        Title = "User is banned",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Inactive:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_INACTIVE",
                        Status = 403,
                        Title = "User is inactive",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Archived:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_ARCHIVED",
                        Status = 403,
                        Title = "User is archived",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Deleted:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_DELETED",
                        Status = 403,
                        Title = "User is deleted",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.ToDelete:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_TO_DELETE",
                        Status = 403,
                        Title = "User is marked for deletion",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            // Return user data
            return new Result<User?>
            {
                Success = true,
                Code = "SESSION_TOKEN_VALID",
                Status = 200,
                Title = "Session token is valid",
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
                Code = "ERROR_VALIDATING_SESSION_TOKEN",
                Status = 400,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Starts the login process by sending a verification code to the user's email. It takes the device id from the
    /// header and the login request from the body. It returns an IActionResult with some relevant data as ok, code, and
    /// status
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<string>> StartLoginProcessAsync(string deviceId, LoginRequest request)
    {
        try
        {
            // Check the request
            if (string.IsNullOrWhiteSpace(request.Email))
                return new Result<string>
                {
                    Success = false,
                    Code = "INVALID_EMAIL",
                    Status = 400,
                    Title = "Email is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Get the user
            var filters = StatusFilters.Create()
                .ThenIncludeUnverified()
                .ThenIncludeBanned()
                .ThenIncludeInactive()
                .ThenIncludeArchived();
            var userResult = await GetByEmailAsync(request.Email, filters);
            if (!userResult || userResult.Data is null)
                return userResult.ConvertTo<string>();
            var user = userResult.Data;

            switch (user.Status)
            {
                // Check the user
                case EntityStatus.Unverified:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_UNVERIFIED",
                        Status = 403,
                        Title = "User is unverified",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Banned:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_BANNED",
                        Status = 403,
                        Title = "User is banned",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Inactive:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_INACTIVE",
                        Status = 403,
                        Title = "User is inactive",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Archived:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_ARCHIVED",
                        Status = 403,
                        Title = "User is archived",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Deleted:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_DELETED",
                        Status = 403,
                        Title = "User is deleted",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.ToDelete:
                    return new Result<string>
                    {
                        Success = false,
                        Code = "USER_TO_DELETE",
                        Status = 403,
                        Title = "User is marked for deletion",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            // Generate and send the code
            user.VerificationCode = SecurityService.GenerateVerificationCode();
            user.VerificationCodeDate = DateTime.UtcNow;

            var emailResult = await _notificationService.SendLoginCodeAsync(
                user.Name,
                request.Email,
                user.VerificationCode
            );
            if (!emailResult)
                return emailResult.ConvertTo<string>();

            // Update the verification code and date
            var updateResult = await _userRepo.UpdateAsync(user);
            if (!updateResult || updateResult.Data is null)
                return updateResult.ConvertTo<string>();

            return new Result<string>
            {
                Success = true,
                Code = "LOGIN_CODE_SENT",
                Status = 200,
                Title = "Login code sent successfully",
                Data = updateResult.Data.Email,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error starting login process for user with email: {request.Email}");
            return new Result<string>
            {
                Success = false,
                Code = "ERROR_STARTING_LOGIN_PROCESS",
                Status = 400,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<User?>> LoginWithCodeAsync(string deviceId, LoginWithCodeRequest request)
    {
        try
        {
            // Check the request
            if (string.IsNullOrWhiteSpace(request.Email))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_EMAIL",
                    Status = 400,
                    Title = "Email is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (string.IsNullOrWhiteSpace(request.Code))
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_LOGIN_CODE",
                    Status = 400,
                    Title = "Login code is required",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // Get the user
            var filters = StatusFilters.Create()
                .ThenIncludeUnverified()
                .ThenIncludeBanned()
                .ThenIncludeInactive()
                .ThenIncludeArchived();
            var userResult = await GetByEmailAsync(request.Email, filters, includeSessionToken: true);
            if (!userResult || userResult.Data is null)
                return userResult.ConvertTo<User?>();
            var user = userResult.Data;

            switch (user.Status)
            {
                // Check the user
                case EntityStatus.Unverified:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_UNVERIFIED",
                        Status = 403,
                        Title = "User is unverified",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Banned:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_BANNED",
                        Status = 403,
                        Title = "User is banned",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Inactive:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_INACTIVE",
                        Status = 403,
                        Title = "User is inactive",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Archived:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_ARCHIVED",
                        Status = 403,
                        Title = "User is archived",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.Deleted:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_DELETED",
                        Status = 403,
                        Title = "User is deleted",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
                case EntityStatus.ToDelete:
                    return new Result<User?>
                    {
                        Success = false,
                        Code = "USER_TO_DELETE",
                        Status = 403,
                        Title = "User is marked for deletion",
                        TraceCode = FileCodes.CallerIC(),
                        Returnable = true
                    };
            }

            // Check
            if (user.VerificationCodeDate == null || user.VerificationCodeDate.Value.AddMinutes(15) < DateTime.UtcNow)
                return new Result<User?>
                {
                    Success = false,
                    Code = "LOGIN_CODE_EXPIRED",
                    Status = 400,
                    Title = "Login code has expired. Please request a new one.",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            if (request.Code != user.VerificationCode)
                return new Result<User?>
                {
                    Success = false,
                    Code = "INVALID_LOGIN_CODE",
                    Status = 400,
                    Title = "Invalid login code",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };

            // The code is correct so login the user
            user.VerificationCode = string.Empty;
            user.VerificationCodeDate = null;

            // Create token
            user.SessionToken ??= new SessionToken();
            user.SessionToken.UserId = user.Id;
            user.SessionToken.Renew(deviceId);

            var updateResult = await _userRepo.UpdateAsync(user);
            if (!updateResult || updateResult.Data is null) return updateResult.ConvertTo<User?>();
            user = updateResult.Data;
            return new Result<User?>
            {
                Success = true,
                Code = "LOGIN_SUCCESSFUL",
                Status = 200,
                Title = "Login successful",
                Data = user,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, $"Error starting login process for user with email: {request.Email}");
            return new Result<User?>
            {
                Success = false,
                Code = "ERROR_LOGGING_IN",
                Status = 400,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    #endregion
}