using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;

namespace Backend.Core.Repositories.Interfaces;

public interface IPetRepository
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
    /// Signs up a new pet. It returns an <see cref="Result{Pet}"/> indicating the result of the operation and including
    /// the pet if it was success.
    /// </summary>
    /// <param name="pet">The pet to add</param>
    /// <returns>A <see cref="Result"/> indicating whether the creation was successful</returns>
    public Task<Result<Pet?>> AddAsync(Pet pet);

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
    public Task<Result<OwnershipInvitation?>> AddOwnershipInvitationAsync(OwnershipInvitation invitation);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously gets an ownership invitation by its ID, applying optional status filters to the query.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="filters"></param>
    /// <param name="includeUsers"></param>
    /// <param name="includePet"></param>
    /// <returns></returns>
    public Task<Result<OwnershipInvitation?>> GetOwnershipInvitationByIdAsync(
        int id,
        StatusFilters? filters = null,
        bool includeUsers = true,
        bool includePet = true
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets an ownership invitation by its invitation code, applying optional status filters to the query. The invitation codeis hashed and
    /// compared to the stored hash in the database to find the invitation. This is done to avoid storing the invitation codein
    /// plaintext in the database for security reasons. If an ownership invitation with the provided invitation codeis found,
    /// it is decrypted and returned. Otherwise, an error result is returned indicating that no ownership invitation
    /// was found with the provided invitationCode. This method is useful for accepting ownership invitations, where the invitationCode
    /// is provided in the invitation link and needs to be validated and used to retrieve the corresponding ownership
    /// invitation.
    /// </summary>
    /// <param name="invitationCode"></param>
    /// <param name="filters"></param>
    /// <param name="includeUsers"></param>
    /// <param name="includePet"></param>
    /// <returns></returns>
    public Task<Result<OwnershipInvitation?>> GetOwnershipInvitationByCodeAsync(
        string invitationCode,
        StatusFilters? filters = null,
        bool includeUsers = true,
        bool includePet = true
    );

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously deletes an ownership invitation by its ID. This method first retrieves the ownership invitation
    /// from the database using the provided ID. If the ownership invitation is found, it is removed from the database
    /// context and the changes are saved to the database. If the deletion is successful, a success result is returned.
    /// If the ownership invitation is not found or if an error occurs during deletion, an appropriate error result is
    /// returned indicating the reason for the failure. This method is useful for allowing users to delete ownership
    /// invitations that they have created or received, providing them with control over their shared pets and
    /// invitations.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Result> DeleteOwnershipInvitationAsync(int id);
}