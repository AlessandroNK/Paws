using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Pet;

public class Pet
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the pet in the database
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// The date and time when the pet was created. It is used to track when the pet was created and to sort pets by
    /// creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the pet was last updated. It is used to track when the pet was last updated and to sort
    /// pets
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// A flag to track pet status in the system.
    /// </summary>
    public PetStatus Status { get; set; }


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

}