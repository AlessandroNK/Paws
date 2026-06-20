using Backend.Core.Models.Pets;
using Backend.Core.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Core.Controllers.interfaces;

public interface IPetController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


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
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IActionResult> AddNewPetAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AddNewPetRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Removes a pet from the user. It takes the device id from the header and the remove pet from user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the removed pet data. It also
    /// checks if the user is verified before removing the pet from the user. If the user is not verified, it returns a
    /// bad request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IActionResult> RemovePetAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] RemovePetRequest request
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shares the ownership of a pet with another user by sending an ownership invitation.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="invitationRequest">The <see cref="SendOwnershipInvitationRequest"/> request</param>
    /// <returns></returns>
    /// <returns></returns>
    public Task<IActionResult> SendOwnershipInvitationAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SendOwnershipInvitationRequest invitationRequest
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Accepts an ownership invitation for a pet.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="invitationCode"></param>
    /// <returns></returns>
    public Task<IActionResult> AcceptOwnershipInvitation(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] AcceptOwnershipInvitationRequest invitationCode
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets all pets by owner.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="sessionToken">The session token of the user sending the request</param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IActionResult> GetPetsByOwnerAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromHeader(Name = "session-Token")] string sessionToken,
        [FromBody] GetPetsByOwnerRequest request
    );
}