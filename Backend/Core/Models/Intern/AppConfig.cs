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
    /// EF primary key so world does not explote
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The configuration key. This is an enum value that identifies the specific configuration setting.
    /// </summary>
    public AppConfigKeys Key { get; set; }

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