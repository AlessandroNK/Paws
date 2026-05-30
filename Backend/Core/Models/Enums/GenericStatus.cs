namespace Backend.Core.Models.Enums;

public enum GenericStatus
{
    /// <summary>
    /// The default status of an entity. It is used to indicate that the entity is active and can be used in the system.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The status of an entity that has been deleted. It is used to indicate that the entity has been deleted and should
    /// not be used in the system.
    /// </summary>
    Deleted = 2,

     /// <summary>
    /// The status of an entity that has been banned. It is used to indicate that the entity has been banned and should
    /// not be used in the system.
    /// </summary>
    Banned = 3,

     /// <summary>
    /// The status of an entity that has been archived. It is used to indicate that the entity has been archived and should
    /// not be used in the system.
    /// </summary>
    Archived = 4,
}