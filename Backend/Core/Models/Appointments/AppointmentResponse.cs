using Backend.Core.Models.Vets;

namespace Backend.Core.Models.Appointments;

public class AppointmentResponse
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>Gets or sets the appointment identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the assigned veterinarian identifier.</summary>
    public int VetId { get; set; }

    /// <summary>Gets or sets the associated veterinarian.</summary>
    public VetResponse? Vet { get; set; }

    /// <summary>Gets or sets the pet identifier.</summary>
    public int? PetId { get; set; } = null;

    /// <summary>Gets or sets the appointment start date and time.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Gets or sets the appointment end date and time.</summary>
    public DateTime EndTime { get; set; }

    /// <summary>Gets or sets when the appointment was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets when the appointment was last updated.</summary>
    public DateTime UpdatedAt { get; set; }


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