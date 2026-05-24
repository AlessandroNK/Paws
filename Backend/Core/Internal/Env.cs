namespace Backend.Core.Internal;

/// <summary>
/// A class that does things (I'm tired, don't judge me for this documentation)
/// </summary>
public static class Env
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This one is easy to guess don't mames
    /// </summary>
    public static Version Version { get; } = new Version(0, 0, 1);


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
    /// <summary>
    /// Returns the version of the application as a string.
    /// </summary>
    /// <returns></returns>
    public static string GetVersion() => Version.ToString();
}