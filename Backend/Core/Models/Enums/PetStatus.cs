namespace Backend.Core.Models.Enums;

public enum PetStatus
{
    /// <summary>
    /// The pet is active and can get appointments.
    /// </summary>
    Active,

    /// <summary>
    /// The pet is inactive and cannot get appointments.
    /// </summary>
    Inactive,

    /// <summary>
    /// This pet was banned so send it to fkck itself
    /// </summary>
    Banned
}