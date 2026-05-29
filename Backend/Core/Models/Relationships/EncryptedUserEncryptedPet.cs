using Backend.Core.Models.Pet;
using Backend.Core.Models.User;

namespace Backend.Core.Models.Relationships;

public class EncryptedUserEncryptedPet
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the user-pet relationship in the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The id of the user in the relationship.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user of the relationship
    /// </summary>
    public EncryptedUser User { get; set; }

    /// <summary>
    /// The id of the pet in the relationship.
    /// </summary>
    public int PetId { get; set; }

    /// <summary>
    /// The pet of the relationship
    /// </summary>
    public EncryptedPet Pet { get; set; }


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