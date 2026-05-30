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
    /// Checks a create pet request
    /// </summary>
    /// <param name="request">The crate pet request to check</param>
    /// <returns>A result indicating whether the request is valid or not</returns>
    public Result CheckCreatePetRequest(CreatePetRequest request);
}