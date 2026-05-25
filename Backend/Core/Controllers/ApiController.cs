using Backend.Core.Controllers.interfaces;
using Backend.Core.Internal;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
    [Route("version")]
    public IActionResult GetVersion() => Ok(Env.GetVersion());
}