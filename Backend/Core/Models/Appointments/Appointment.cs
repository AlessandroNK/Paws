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
    /// <summary>
    /// Gets or sets the unique identifier for the appointment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the veterinarian ID associated with this appointment.
    /// </summary>
    public int VetId { get; set; }

    /// <summary>
    /// The vet object
    /// </summary>
    public Vet Vet { get; set; }

    /// <summary>
    /// Gets or sets the user's pet ID for which the appointment is scheduled.
    /// </summary>
    public int UserPetId { get; set; }

    /// <summary>
    /// The UserPet object
    /// </summary>
    public UserPet UserPet { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the appointment.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the appointment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the appointment was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the current status of the appointment.
    /// </summary>
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