namespace Backend.Core.Models.Enums;

public enum PetStatus
{
    /// <summary>
    /// The pet is active and can get appointments.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The pet is inactive and cannot get appointments.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// This pet was banned so send it to fkck itself
    /// </summary>
    Banned = 3
}