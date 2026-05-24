using Backend.Core.DTOs.Requests;
using Backend.Core.Internal;
using Backend.Core.Models;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

/// <summary>
/// Provides functionality for managing user operations.
/// </summary>
public class UserService(ILogger<UserService> logger) : IUserService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    private readonly ILogger<UserService> _logger = logger;


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
    /// Signs up a new user. It takes the device id from the header and the sign up request from the body. It returns an
    /// IActionResult with some relevant data as ok, code, and status
    /// </summary>
    /// <param name="deviceId">The device id of the user</param>
    /// <param name="request">The sign up request</param>
    /// <returns></returns>
    public async Task<Result<User>> SignUp(string deviceId, SignUpRequest request)
    {
        try
        {
            _logger.LogInformation("Signing up user");

            return new Result<User>
            {
                Ok = true,
                Code = "USER_SIGNED_UP",
                Status = 200,
                Message = "User signed up successfully",
                Data = new User
                {
                    Id = 0,
                    Email = request.Email,
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception e)
        {
            Helpers.LogError(_logger, e, "Error signing up user");
            return new Result<User>
            {
                Ok = false,
                Code = "ERROR_SIGNING_UP_USER",
                Status = 500,
                Message = "An error occurred while signing up the user"
            };
        }
    }
}