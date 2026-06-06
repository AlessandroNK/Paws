using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Intern;

public class SetConfigRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The key of the configuration to set. It will be used to identify the configuration in the database and to retrieve it later.
    /// </summary>
    [Required(ErrorMessage = "Key id is required.")]
    public int Key { get; set; }

    /// <summary>
    /// The value of the configuration to set. It will be used to update the configuration in the database.
    /// </summary>
    [Required(ErrorMessage = "Value is required.")]
    public string Value { get; set; }


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