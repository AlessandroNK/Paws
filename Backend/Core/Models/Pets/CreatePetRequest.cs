using System.ComponentModel.DataAnnotations;
using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Pets;

public class CreatePetRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The name of the pet.
    /// </summary>
    [Required(ErrorMessage = "Pet name is required.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The species of the pet
    /// </summary>
    [Required(ErrorMessage = "Pet species is required.")]
    public PetSpecies Species { get; set; } = PetSpecies.Other;

    /// <summary>
    /// The breed of the pet. it will depend on
    /// </summary>
    public string Breed { get; set; } = string.Empty;


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