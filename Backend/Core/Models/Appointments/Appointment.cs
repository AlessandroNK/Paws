using Backend.Core.Models.Enums;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Vets;

namespace Backend.Core.Models.Appointments;

/// <summary>
/// Represents a veterinary appointment for a user's pet.
/// </summary>
public class Appointment
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
    public Vet? Vet { get; set; }

    /// <summary>Gets or sets the user's pet identifier.</summary>
    public int UserPetId { get; set; }

    /// <summary>Gets or sets the associated user pet.</summary>
    public UserPet? UserPet { get; set; }

    /// <summary>Gets or sets the appointment start date and time.</summary>
    public DateTime Start { get; set; }

    /// <summary>Gets or sets the appointment end date and time.</summary>
    public DateTime End { get; set; }

    /// <summary>Gets or sets when the appointment was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets when the appointment was last updated.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the current appointment status.</summary>
    public AppointmentStatus Status { get; set; }


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