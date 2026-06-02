namespace Backend.Core.Models.Intern;

/// <summary>
/// Represents a simple application configuration entry — a key/value pair used
/// by various parts of the application to hold configuration values that may
/// come from AppSettings, environment variables, database, or other stores.
/// </summary>
public class AppConfig
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The configuration key. Typical values are hierarchical keys like
    /// "Logging:LogLevel:Default" or custom application keys. Keys are treated
    /// as identifiers and should not be null. Use <see cref="string.Empty"/>
    /// when no value is provided.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The configuration value associated with <see cref="Key"/>. This may be
    /// an empty string when the key exists but no value is set.
    /// </summary>
    public string Value { get; set; } = string.Empty;


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