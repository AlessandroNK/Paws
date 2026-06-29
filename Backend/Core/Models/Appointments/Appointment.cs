using Backend.Core.Models.Enums;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Vets;

namespace Backend.Core.Models.Appointments;

/// <summary>
/// Represents a veterinary appointment for a user's pet.
/// </summary>
public class Appointment : IDtoConvertible<AppointmentResponse>
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

    /// <summary>Gets or sets the pet identifier.</summary>
    public int? PetId { get; set; } = null;

    /// <summary>
    /// This pet object is optional and can be null.
    /// </summary>
    public Pet? Pet { get; set; }

    /// <summary>Gets or sets the appointment start date and time.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Gets or sets the appointment end date and time.</summary>
    public DateTime EndTime { get; set; }

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
    AppointmentResponse? IDtoConvertible<AppointmentResponse>.ToDto()
    {
        return  new AppointmentResponse
        {
            Id = Id,
            VetId = VetId,
            Vet = Vet?.ToDto(),
            PetId = PetId,
            StartTime = StartTime,
            EndTime = EndTime,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public AppointmentResponse ToDto()
    {
        return  new AppointmentResponse
        {
            Id = Id,
            VetId = VetId,
            Vet = Vet?.ToDto(),
            PetId = PetId,
            StartTime = StartTime,
            EndTime = EndTime,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public AppointmentBasicResponse ToScheduleDto()
    {
        return new AppointmentBasicResponse
        {
            Id = Id,
            VetId = VetId,
            Vet = Vet?.ToBasicDto(),
            StartTime = StartTime,
            EndTime = EndTime
        };
    }
}