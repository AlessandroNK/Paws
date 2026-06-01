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
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public Task<Result<Pet?>> GetByIdAsync(int id, StatusFilters? filters = null);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates a pet in the system. It takes the pet data and returns a result with the updated pet or an error if
    /// something went wrong.
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    public Task<Result<Pet?>> UpdateAsync(Pet pet);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result<ShareInvitation?>> AddShareInvitationAsync(ShareInvitation invitation);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously gets a share invitation by its ID, applying optional status filters to the query.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public Task<Result<ShareInvitation?>> GetShareInvitationByIdAsync(int id, StatusFilters? filters = null);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a share invitation by its nonce, applying optional status filters to the query. The nonce is hashed and
    /// compared to the stored hash in the database to find the invitation. This is done to avoid storing the nonce in
    /// plaintext in the database for security reasons. If a share invitation with the provided nonce is found, it is
    /// decrypted and returned. Otherwise, an error result is returned indicating that no share invitation was found
    /// with the provided nonce. This method is useful for accepting share invitations, where the nonce is provided in
    /// the invitation link and needs to be validated and used to retrieve the corresponding share invitation.
    /// </summary>
    /// <param name="nonce"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public Task<Result<ShareInvitation?>> GetShareInvitationByNonceAsync(string nonce, StatusFilters? filters = null);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Asynchronously deletes a share invitation by its ID. This method first retrieves the share invitation from the
    /// database using the provided ID. If the share invitation is found, it is removed from the database context and
    /// the changes are saved to the database. If the deletion is successful, a success result is returned. If the share
    /// invitation is not found or if an error occurs during deletion, an appropriate error result is returned indicating
    /// the reason for the failure. This method is useful for allowing users to delete share invitations that they have
    /// created or received, providing them with control over their shared pets and invitations.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Result> DeleteShareInvitationAsync(int id);
}