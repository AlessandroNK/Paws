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
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The species of the pet
    /// </summary>
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