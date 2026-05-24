namespace Backend.Core.Models.Enums;

public enum UserStatus
{
    /// <summary>
    /// The user is active and can use the API
    /// </summary>
    Active = 0,

    /// <summary>
    /// The user is inactive and cannot use the API. It is used to soft delete users.
    /// </summary>
    Inactive = 1,

     /// <summary>
    /// The user is banned and cannot use the API. It is used to ban users.
    /// </summary>
    Banned = 2,

     /// <summary>
     /// The user is unverified and cannot use the API. It is used to mark users that have not verified their email or phone
     /// </summary>
     Unverified = 3,
}