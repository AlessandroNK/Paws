using Backend.Core.Internal;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Results;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Models.Users;

public class SessionToken : IEncryptable
{
    /// <summary>
    /// The id of the session token in the database, this is used to identify the session token in the database and to
    /// invalidate it when the user logs out from a device or when the user logs out from all devices.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The status of the session token, this is used to invalidate the session token when the user logs out from a device
    /// or when the user logs out from all devices, if the status is inactive, the session token is invalidated and the
    /// user will be logged out from the device, if the status is active, the session token is valid and the user will be
    /// able to access the app from the device.
    /// </summary>
    public EntityStatus Status { get; set; }

    /// <summary>
    /// The id of the user in the database
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user that is associated with the session token, this is used to validate the token and to get the user's information when the
    /// user tries to access the app from a device.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// The token of the session
    /// </summary>
    [EncryptProperty]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The hash of the token, this is used to validate the token when the user tries to access the app from a device,
    /// this is used to prevent the user from using a token that has been invalidated by logging out from a device or
    /// from all devices.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The id of the device that is using the session token, this is used to invalidate the session token when the user
    /// logs out from a device or when the user logs out from all devices
    /// </summary>
    [EncryptProperty]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// The date when the session token was created, this is used to invalidate the session token after a certain amount
    /// of time
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date when the session token will expire, this is used to invalidate the session token after a certain amount
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    // -----------------------------------------------------------------------------------------------------------------
    public void Renew()
    {
        Token = $"{UserId}::{DeviceId}::{Guid.NewGuid().ToString()}";
        ExpiresAt = DateTime.UtcNow.AddDays(7);
    }

    // -----------------------------------------------------------------------------------------------------------------
    public Result Hash()
    {
        if (string.IsNullOrWhiteSpace(Token))
            return new Result
            {
                Success = false,
                Code = "TOKEN_NOT_PROVIDED",
                Status = 400,
                Title = "Token not provided for session token hashing",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };

        var tokenResult = SecurityService.HashWithSalt(Token);
        if (!tokenResult.Success || tokenResult.Data is null)
            return new Result
            {
                Success = false,
                Code = "TOKEN_HASHING_FAILED",
                Status = 500,
                Title = "Failed to hash session token",
                TraceCode = $"{FileCodes.CallerIC()}",
                Returnable = false
            };
        TokenHash = tokenResult.Data;

        return new Result
        {
            Success = true,
            Code = "TOKEN_HASHED",
            Status = 200,
            Title = "Token hashed successfully",
            TraceCode = $"{FileCodes.CallerIC()}",
            Returnable = false
        };
    }
}