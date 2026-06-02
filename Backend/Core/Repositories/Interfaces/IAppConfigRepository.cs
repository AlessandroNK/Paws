using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;

namespace Backend.Core.Repositories.Interfaces;

public interface IAppConfigRepository
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
    /// Gets the app configurations from the database. It returns a <see cref="Result{List{AppConfig}}"/> indicating the
    /// result of the operation and including the list of app configurations if they were found.
    /// </summary>
    /// <returns></returns>
    public Task<Result<List<AppConfig>>> GetConfigs();

    // -----------------------------------------------------------------------------------------------------------------
    public Task<Result> SetConfig(string key, string value);
}