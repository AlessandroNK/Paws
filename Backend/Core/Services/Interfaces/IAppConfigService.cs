using Backend.Core.Models.Intern;
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
    /// <summary>
    /// Sets a configuration value by its key. It updates the value in the database and then updates the in-memory dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task<Result> SetConfig(AppConfigKeys key, string value);

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a configuration value by its key from the in-memory dictionary. If the key is not found, it returns a result
    /// indicating that the config was not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Result<string> GetConfig(AppConfigKeys key);
}