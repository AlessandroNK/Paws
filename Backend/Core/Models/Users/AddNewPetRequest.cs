using System.ComponentModel.DataAnnotations;
using Backend.Core.Models.Pets;

namespace Backend.Core.Models.Users;

public class AddNewPetRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the user asking to append the pet.
    /// </summary>
    [Required(ErrorMessage = "UserId is required.")]
    public int UserId { get; set; }

    /// <summary>
    /// The request to create a pet
    /// </summary>
    [Required(ErrorMessage = "Pet information is required.")]
    public CreatePetRequest Pet { get; set; }


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

}