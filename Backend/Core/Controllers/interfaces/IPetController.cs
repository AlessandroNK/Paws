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
    /// Shares the ownership of a pet with another user. It sends an invitation to the user to accept the ownership of
    /// the pet.
    /// </summary>
    /// <param name="deviceId">The id of the device sending the request</param>
    /// <param name="request">The <see cref="SharePetOwnershipRequest"/> request</param>
    /// <returns></returns>
    public Task<IActionResult> SendOwnershipInvitationAsync(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SharePetOwnershipRequest request
    );
}