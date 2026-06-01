using Backend.Core.Models.Pets;
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
}