using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Vets;

public struct VetResponse
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The id of the user in the database
    /// </summary>
    public int Id  { get; init; }

    /// <summary>
    /// The email in a readable string to return to the frontend
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The type of the document of the user.
    /// </summary>
    public DocumentType DocumentType { get; init; }

    /// <summary>
    /// The number of identification like we are simple elements named by random numbers
    /// </summary>
    public string DocumentNumber { get; set; }

    /// <summary>
    /// The name of the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The professional license number issued to the veterinarian by the
    /// relevant authority or licensing board.
    /// </summary>
    public string ProfessionalLicenseNumber { get; set; }

    /// <summary>
    /// The date when the veterinarian started practicing veterinary medicine or obtained their veterinary license.
    /// This information can be used to determine the veterinarian's experience and expertise in the field.
    /// </summary>
    public DateTime VetSince { get; set; }


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