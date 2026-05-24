namespace Backend.Core.DTOs.Requests;

/// <summary>
/// Represents a sign out request. It contains the user's id. It is used to sign out an existing user. It is used in the
/// sign out endpoint of the user controller.
/// </summary>
public class SignOutRequest
{
        /// <summary>
        /// The id of the user that wants to sign out
        /// </summary>
        public required int UserId { get; set; }
}