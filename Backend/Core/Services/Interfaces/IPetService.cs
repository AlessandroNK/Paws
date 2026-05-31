using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;

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
    /// <param name="excludeInactive">Whether to exclude inactive pets</param>
    /// <param name="excludeBanned">Whether to exclude banned pets</param>
    /// <returns>A <see cref="Result{Pet}"/> indicating the result of the operation and including the pet if it was found</returns>
    public Task<Result<Pet?>> GetByIdAsync(int id, bool excludeInactive = true, bool excludeBanned = true);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates a pet in the system. It takes the pet data and returns a result with the updated pet or an error if
    /// something went wrong.
    /// </summary>
    /// <param name="pet"></param>
    /// <returns></returns>
    public Task<Result<Pet?>> UpdateAsync(Pet pet);

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result> SharePetOwnershipAsync(SharePetOwnershipRequest request);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks a create pet request
    /// </summary>
    /// <param name="request">The crate pet request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    public Result CheckCreatePetRequest(CreatePetRequest request);
}