using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

public class AppConfigRepository(
    ApplicationDbContext dbContext,
    ILogger<AppConfigRepository> logger
) : IAppConfigRepository
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The database context used to interact with the database.
    /// This context is used to perform CRUD operations on the <see cref="User"/> and
    /// <see cref="DocumentType"/> entities.
    /// It is injected into the repository to provide access to the underlying database.
    /// </summary>
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<AppConfigRepository> _logger = logger;


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
    public async Task<Result<List<AppConfig>>> GetConfigs()
    {
        var configs = await _dbContext.AppConfigs.ToListAsync();

        if (configs.Count != 0)
            return new Result<List<AppConfig>>
            {
                Success = true,
                Code = "CONFIGS_RETRIEVED",
                Status = 200,
                Message = "App configs retrieved successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true,
                Data = configs
            };

        LogHelpers.LogError(_logger, "No app configs found in the database");
        return new Result<List<AppConfig>>
        {
            Success = false,
            Code = "NO_APP_CONFIGS_FOUND",
            Status = 404,
            Message = "No app configs found in the database",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }
}