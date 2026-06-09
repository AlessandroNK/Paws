using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Models.Appointments;

public class AppointmetDayRequest
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    [Required(ErrorMessage = "Year is required.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Month is required.")]
    public int Month { get; set; }

    [Required(ErrorMessage = "Day is required.")]
    public int Day { get; set; }

    // TODO add local Time Zone prop


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