using System.Collections.Generic;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Users;

namespace Backend.Core.Models.Vets;

/// <summary>
/// Represents a veterinarian user in the system. Inherits from <see cref="User"/> and
/// contains vet-specific information such as professional license number and
/// a collection of associated <see cref="Appointment"/> instances.
/// </summary>
public class Vet : User, IDtoConvertible<VetResponse>
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The professional license number issued to the veterinarian by the
    /// relevant authority or licensing board.
    /// </summary>
    public string ProfessionalLicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// A list of appointments associated with this veterinarian. The list is
    /// initialized to an empty collection to avoid null reference issues.
    /// </summary>
    public List<Appointment> Appointments { get; set; } = new();


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
    VetResponse IDtoConvertible<VetResponse>.ToDto()
    {
        return new VetResponse
        {
            Id = Id,
            Email = Email,
            DocumentType = DocumentType,
            DocumentNumber = DocumentNumber,
            Name = Name,
            ProfessionalLicenseNumber = ProfessionalLicenseNumber
        };
    }
}