using System.Text.Json;
using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Paws;
using Backend.Core.Models.Results;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Backend.Core.Controllers;

/// <summary>
/// This controller handles internal base operations for this API.
/// </summary>
/// <remarks>FI01</remarks>
[ApiController]
[Route("[controller]")]
[EnableCors("AllowFrontend")]
public class ApiController(
    IAppConfigService appConfigService,
    ILogger<UserController> logger
) : ControllerBase, IApiController
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    private IAppConfigService _appConfigService = appConfigService;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<UserController> _logger = logger;


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
    /// This endpoint returns the version of the API. It is used to check if the API is up and running and to check if
    /// the version of the API is compatible with the client. It is also used to check if the API is up and running.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("chat")]
    public async Task<IActionResult> ChatWithPaws([FromBody] ChatRequest request)
    {
        // TODO clean this shit up
        IChatClient chatClient = new OllamaApiClient("http://localhost:11434", "llama3.1:8b");
        chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(
                    () => Env.GetVersion(),
                    new AIFunctionFactoryOptions
                    {
                        Name = "get_version",
                        Description =
                            "Returns the version of the API. It is used to check if the API is up and running and to check if the version of the API is compatible with the client. It is also used to check if the API is up and running."
                    }
                ),
                AIFunctionFactory.Create(
                    () => Env.GetAppContext(),
                    new AIFunctionFactoryOptions
                    {
                        Name = "get_app_context",
                        Description =
                            "Returns the application context. It is used to check the environment and configuration of the API. It is also used to check if the API is up and running."
                    }
                ),
                AIFunctionFactory.Create(
                    () => Env.GetProjectContext(),
                    new AIFunctionFactoryOptions
                    {
                        Name = "get_project_context",
                        Description =
                            "Do you need to know something about Paws? This function returns information about what Paws is. It is used to provide context about the project, such objectives, Features , Goals, Limitations, Purpose."
                    }
                )
            ]
        };

        // Prompt and context for the AI
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, """
                                 You are Paws, the charismatic official pet (mascota) of the PAWS app.
                                 You MUST match the user's message and respond in a way that reflects the mood of the user.
                                 You MUST match the user's language.
                                 You MUST respond ONLY with a valid JSON object in this exact format, no markdown, no extra text:
                                 {
                                   "message": "<your response here>",
                                   "mood": "<mood name here>"
                                 }
                                 
                                 The mood field MUST be one of these exact strings (not numbers):
                                 Happy, Sad, Playful, Hungry, Sleepy, Excited, Calm, Chill, Confident.
                                 
                                 Choose the mood that genuinely reflects how you feel while generating this response.
                                 No markdown, no extra text — raw JSON only.
                                 """),
            new(ChatRole.User, request.Message)
        };
        var response = await chatClient.GetResponseAsync(messages, chatOptions);
        var rawOne = response.Text;

        // Parse response into JSON to get the object
        PawsChatResponse pawsResponse;
        try
        {
            var parsed = JsonSerializer.Deserialize<PawsChatRaw>(
                rawOne,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            Console.WriteLine(parsed.Message);
            pawsResponse = new PawsChatResponse
            {
                Message = parsed!.Message,
                Mood = parsed.Mood
            };
        }
        catch (Exception e)
        {
            Console.WriteLine("FAILED TO PARSE PAWS RESPONSE, INSULTING THE MODEL");
            Console.WriteLine(e.Message);
            // Model didn't respect the format, I need to insult it
            pawsResponse = new PawsChatResponse
            {
                Message = rawOne,
                Mood = nameof(PawsMood.Happy)
            };
        }

        var result = new Result<PawsChatResponse>
        {
            Success = true,
            Code = "CHAT_RESPONSE",
            Status = 200,
            Data = pawsResponse,
            Message = "Chat response generated successfully",
            TraceCode = FileCodes.CallerIC()
        };
        return Ok(result);
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This endpoint returns the version of the API. It is used to check if the API is up and running and to check if
    /// the version of the API is compatible with the client. It is also used to check if the API is up and running.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("version")]
    public IActionResult GetVersion() => Ok(
        new Result<string>
        {
            Success = true,
            Status = 200,
            Code = "API_VERSION",
            Data = Env.GetVersion(),
            Message = "API version retrieved successfully",
            TraceCode = FileCodes.CallerIC()
        });

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This endpoint sets a configuration value by its key. It updates the value in the database and then updates the
    /// in-memory dictionary. It takes the device id from the header and the set config request from the body. It returns
    /// an IActionResult with some relevant data as ok, code, and the set config data.
    /// </summary> <param name="deviceId"></param>
    /// <param name="request"></param>
    [HttpPost]
    [Route("set-config")]
    public async Task<IActionResult> SetConfig(
        [FromHeader(Name = "Device-Id")] string deviceId,
        [FromBody] SetConfigRequest request
    )
    {
        _logger.LogInformation(
            "Received request to set configuration for key {@Key} with value {@Value}",
            request.Key, request.Value
        );

        // Validations
        Env.SetInteractionCode(deviceId);
        var deviceValidationResult = SecurityService.ValidateDeviceId(deviceId);
        if (!deviceValidationResult) return BadRequest(deviceValidationResult);

        // Send the ownership invitation
        var result = await _appConfigService.SetConfig(request);

        // Clean the response and convert it and its data to Dto
        return result
            ? Ok(result)
            : BadRequest(result);
    }
}