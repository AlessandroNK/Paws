using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Result;
using Backend.Core.Models.User;
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
    /// The logger used to log messages.
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


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------

    #region UserRelated

    /// <summary>
    /// Finds a user by its email.
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByEmailAsync(
        string email,
        bool excludeInactive = true,
        bool excludeBanned = true
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
                    excludeBanned
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
    /// <param name="excludeInactive">Whether to filter out inactive users</param>
    /// <param name="excludeBanned">Whether to filter out banned users</param>
    /// <returns>The created user</returns>
    public async Task<Result<User?>> GetByDocumentAsync(
        string document,
        bool excludeInactive = true,
        bool excludeBanned = true
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
                operation: () => _userRepo.GetByDocumentAsync(
                    document,
                    excludeInactive,
                    excludeBanned
                ),
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
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public async Task<Result<User?>> SignUp(string deviceId, SignUpRequest request)
    {
        try
        {
            _logger.LogInformation("Signing up user");

            // Verifications
            var requestResult = CheckSignUpRequest(request);
            if (!requestResult) return requestResult.ConvertTo<User?>();

            // Find the user to avoid duplications
            // This is to avoid telling the uer the exact element that already
            // exists so it is a little harder to attack our platform
            var existingEmailResult = await GetByEmailAsync(request.Email);
            var existingDocumentResult = await GetByDocumentAsync(request.DocumentNumber);

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
                Status = UserStatus.Unverified,
                VerificationCode = SecurityService.GenerateVerificationCode()
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

            // Everything went well, return the user
            return new Result<User?>
            {
                Success = true,
                Code = "VERIFICATION_CODE_SENT",
                Status = 201,
                Message = "User signed up successfully",
                Data = user
            };
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

    #endregion

    #region Helpers

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a sign up request
    /// </summary>
    /// <param name="request">The sign up request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    private static Result CheckSignUpRequest(SignUpRequest request)
    {
        // Verifications
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
        if (!passwordResult)
            return passwordResult;

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

        // Everything is valid
        return new Result
        {
            Success = true,
            Code = "SUCCESS_REQUEST",
            Status = 200,
        };
    }

    #endregion
}