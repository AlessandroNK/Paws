using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;

namespace Backend.Core.Services.Interfaces;

public interface IPetService
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
    /// Creates a new pet in the system. It takes the pet data and the user id of the owner. It returns a result with
    /// the created pet or an error if something went wrong.
    /// </summary>
    /// <param name="request">The data of the pet to create</param>
    /// <returns>The created pet or an error if something went wrong</returns>
    public Task<Result<Pet?>> CreatePetAsync(CreatePetRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a pet by its ID.
    /// </summary>
    /// <param name="id">The ID of the pet to retrieve</param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <param name="includeUsers">Whether to include the user data in the query</param>
    /// <param name="includeOwnerInvitations">Whether to include the ownership invitations in the query</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public Task<Result<Pet?>> GetByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUsers = false,
        bool includeOwnerInvitations = false
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates a pet in the system. It takes the pet data and returns a result with the updated pet or an error if
    /// something went wrong.
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    public Task<Result<Pet?>> UpdateAsync(Pet pet);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates an ownership invitation for a pet by sending an invitation email with a code to the new owner. The new
    /// owner can then use the code to accept the invitation and become a co-owner of the pet. This method checks if the
    /// user is the owner of the pet, generates an ownership code, saves it in the database, and sends it to the new
    /// owner's email address. The code is valid for 24 hours.
    /// </summary>
    /// <param name="invitationRequest"></param>
    /// <returns></returns>
    public Task<Result> SendOwnershipInvitationAsync(SendOwnershipInvitationRequest invitationRequest);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a create pet request
    /// </summary>
    /// <param name="request">The crate pet request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    public Result CheckCreatePetRequest(CreatePetRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Accepts an ownership invitation for a pet.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="filters">The filters to apply to the query</param>
    /// <returns></returns>
    public Task<Result> AcceptOwnershipInvitationAsync(
        AcceptOwnershipInvitationRequest request,
        StatusFilters? filters = null
        );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates and adds a pet to the user. It takes the device id from the header and the add pet to user request from
    /// the body. It returns an IActionResult with some relevant data as ok, code, and the created pet data. It also
    /// checks if the user is verified before adding the pet to the user. If the user is not verified, it returns a bad
    /// request with a message indicating that the user is not verified.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<Result<Pet?>> AddNewPetAsync(AddNewPetRequest request);

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
    public Task<Result<Pet?>> RemovePetAsync(RemovePetRequest request);
}