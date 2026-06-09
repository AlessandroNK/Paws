using Backend.Core.Models.Enums;

namespace Backend.Core.Models.Vets;

public struct VetBasicResponse
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
    /// The name of the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A file path to the veterinarian's profile picture. This can be used in the UI to display the veterinarian's
    /// image, making their profile more personalized and recognizable to users.
    /// </summary>
    public string ProfilePicture { get; set; }

    /// <summary>
    /// Any interesting or fun fact about the veterinarian that can be displayed in the UI to make their profile more
    /// engaging and personable. This could include hobbies, unique experiences, or any other information that adds a
    /// personal touch to their profile.
    /// </summary>
    public string FunFact { get; set; }

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