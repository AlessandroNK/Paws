using Backend.Core.Models.Results;

namespace Backend.Core.Services.Interfaces;

public interface IAppConfigService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


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
    /// Initializes the app configuration by fetching the configurations from the database and storing them in a dictionary
    /// for easy access.
    /// </summary>
    /// <returns></returns>
    public Task<Result> InitializeAsync();

    // -----------------------------------------------------------------------------------------------------------------
    // public Task<Result> SetConfig(string key, string value);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a configuration value by its key from the in-memory dictionary. If the key is not found, it returns a result
    /// indicating that the config was not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Result<string> GetConfig(string key);
}