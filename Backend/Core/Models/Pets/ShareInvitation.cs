namespace Backend.Core.Models.Pets;

public class ShareInvitation
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The ID of the shared invitation
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The ID of the user sharing ownership of the pet.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The ID of the pet to share ownership of.
    /// </summary>
    public int PetId { get; set; }

    /// <summary>
    /// A temporary link to share this pet. This lik includes some important information like the id of the pet, the id
    /// of the user who shared it, and new owner's email. It is used to share the pet with other users without giving
    /// them access to the pet's data.
    /// </summary>
    public string? ShareLink { get; set; } = string.Empty;

    /// <summary>
    /// The email of the user to share ownership with.
    /// </summary>
    public string NewOwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The name of the user to share ownership with.
    /// </summary>
    public string NewOwnerName { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when this shared code will expire
    /// </summary>
    public DateTime ShareCodeExpiration { get; set; }


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