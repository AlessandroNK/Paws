using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Backend.Core.Models.Intern;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Backend.Core.Controllers;

/// <summary>
/// This controller handles internal base operations for this API.
/// </summary>
[ApiController]
[Route("[controller]")]
[EnableCors("AllowFrontend")]
public class ApiController : ControllerBase, IApiController
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
    /// This endpoint returns the version of the API. It is used to check if the API is up and running and to check if
    /// the version of the API is compatible with the client. It is also used to check if the API is up and running.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("chat")]
    public async Task<IActionResult> ChatWithPaws([FromBody] ChatRequest request)
    {
        IChatClient chatClient = new OllamaApiClient("http://localhost:11434", "llama3.1:8b");
        chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(
                    () => Env.GetVersion() ,
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

        var response = await chatClient.GetResponseAsync(request.Message, chatOptions);
        return Ok(response.Text);
    }
}