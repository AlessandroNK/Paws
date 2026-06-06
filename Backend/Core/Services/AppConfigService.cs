using Backend.Core.Internal;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Results;
using Backend.Core.Policies;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services.Interfaces;

namespace Backend.Core.Services;

/// <remarks>FC02</remarks>
public class AppConfigService(
    IAppConfigRepository appConfigRepo,
    ILogger<PetService> logger
) : IAppConfigService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A repo to get and change app's configurations
    /// </summary>
    private readonly IAppConfigRepository _appConfigRepo = appConfigRepo;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<PetService> _logger = logger;

    /// <summary>
    /// A dictionary so it is easy to get app's configurations
    /// </summary>
    private readonly Dictionary<AppConfigKeys, string> _appConfigs = new();


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
    public async Task<Result> InitializeAsync()
    {
        try
        {
            LogHelpers.LogInfo(_logger, "Initializing app config...");

            // Get configs from DB
            var configsResult = await DbRetry.ExecuteWithRetry(
                operation: () => _appConfigRepo.GetConfigs(),
                operationName: "Updating pet",
                logger: _logger
            );
            if (!configsResult || configsResult.Data is null) return configsResult;

            foreach (var config in configsResult.Data)
                _appConfigs[config.Key] = config.Value;

            LogHelpers.LogInfo(_logger, "App config initialized successfully");

            return new Result
            {
                Success = true,
                Code = "APP_CONFIG_INITIALIZED",
                Status = 200,
                Message = "App config initialized successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error initializing app config");
            return new Result
            {
                Success = false,
                Code = "ERROR_INITIALIZING_APP_CONFIG",
                Status = 500,
                Message = "An error occurred while initializing app config",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sets a configuration value by its key. It updates the value in the database and then updates the in-memory dictionary
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result> SetConfig(SetConfigRequest request)
    {
        try
        {
            // Validation
            if (!Enum.IsDefined(typeof(AppConfigKeys), request.Key))
            {
                return new Result
                {
                    Success = false,
                    Code = "INVALID_CONFIG_KEY",
                    Status = 400,
                    Message = "Config key is not valid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            if (string.IsNullOrWhiteSpace(request.Value))
            {
                return new Result
                {
                    Success = false,
                    Code = "INVALID_CONFIG_VALUE",
                    Status = 400,
                    Message = "Config value cannot be null or whitespace",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            LogHelpers.LogInfo(_logger, $"Setting app config {request.Key}...");

            AppConfigKeys key;
            try
            {
                key = (AppConfigKeys)request.Key;
            }
            catch (Exception)
            {
                return new Result
                {
                    Success = false,
                    Code = "INVALID_CONFIG_KEY",
                    Status = 400,
                    Message = "Config key is not valid",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            // Update config in DB
            var setConfigResult = await DbRetry.ExecuteWithRetry(
                operation: () => _appConfigRepo.SetConfig(key, request.Value),
                operationName: "Setting app config",
                logger: _logger
            );
            if (!setConfigResult) return setConfigResult;

            // Update config in memory
            _appConfigs[key] = request.Value;

            LogHelpers.LogInfo(_logger, $"App config {key} set successfully");

            return new Result
            {
                Success = true,
                Code = "APP_CONFIG_SET",
                Status = 200,
                Message = $"App config {key} set successfully",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception e)
        {
            LogHelpers.LogError(_logger, e, "Error setting app config");
            return new Result
            {
                Success = false,
                Code = "ERROR_SETTING_APP_CONFIG",
                Status = 500,
                Message = "An error occurred while setting app config",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a configuration value by its key from the in-memory dictionary. If the key is not found, it returns a result
    /// indicating that the config was not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Result<string> GetConfig(AppConfigKeys key)
    {
        if (_appConfigs.TryGetValue(key, out var value))
        {
            return new Result<string>
            {
                Success = true,
                Code = "CONFIG_FOUND",
                Status = 200,
                Message = "Config found",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true,
                Data = value
            };
        }

        return new Result<string>
        {
            Success = false,
            Code = "CONFIG_NOT_FOUND",
            Status = 404,
            Message = "Config not found",
            TraceCode = FileCodes.CallerIC(),
            Returnable = true
        };
    }
}