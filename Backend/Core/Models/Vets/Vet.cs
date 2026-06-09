using System.Collections.Generic;
using Backend.Core.Internal;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Interfaces;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Services;

namespace Backend.Core.Models.Vets;

/// <summary>
/// Represents a veterinarian user in the system. Inherits from <see cref="User"/> and
/// contains vet-specific information such as professional license number and
/// a collection of associated <see cref="Appointment"/> instances.
/// </summary>
public class Vet : User, IDtoConvertible<VetResponse>, IEncryptable
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The professional license number issued to the veterinarian by the
    /// relevant authority or licensing board.
    /// </summary>
    [EncryptProperty]
    public string ProfessionalLicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// The date when the veterinarian started practicing. This can be used to calculate the veterinarian's experience and to determine if they meet certain
    /// requirements for specific appointments or tasks within the system.
    /// </summary>
    public DateTime VetSince { get; set; }

    /// <summary>
    /// A file path to the veterinarian's profile picture. This can be used in the UI to display the veterinarian's
    /// image, making their profile more personalized and recognizable to users.
    /// </summary>
    [EncryptProperty]
    public string ProfilePicture { get; set; }

    /// <summary>
    /// Any interesting or fun fact about the veterinarian that can be displayed in the UI to make their profile more
    /// engaging and personable. This could include hobbies, unique experiences, or any other information that adds a
    /// personal touch to their profile.
    /// </summary>
    [EncryptProperty]
    public string FunFact { get; set; }

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
            ProfessionalLicenseNumber = ProfessionalLicenseNumber,
            VetSince = VetSince,
            ProfilePicture = ProfilePicture,
            FunFact = FunFact
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public VetResponse ToDto()
    {
        return new VetResponse
        {
            Id = Id,
            Email = Email,
            DocumentType = DocumentType,
            DocumentNumber = DocumentNumber,
            Name = Name,
            ProfessionalLicenseNumber = ProfessionalLicenseNumber,
            VetSince = VetSince,
            ProfilePicture = ProfilePicture,
            FunFact = FunFact
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    public VetBasicResponse ToBasicDto()
    {
        return new VetBasicResponse
        {
            Id = Id,
            Name = Name,
            ProfilePicture = ProfilePicture,
            FunFact = FunFact,
            VetSince = VetSince
        };
    }
}