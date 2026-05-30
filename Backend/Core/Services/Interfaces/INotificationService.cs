using Backend.Core.Models.Results;

namespace Backend.Core.Services.Interfaces;

public interface INotificationService
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
    /// Sends a verification code to the given email address.
    /// </summary>
    /// <param name="name">The name of the user</param>
    /// <param name="email">The email to send the code to</param>
    /// <param name="code">The code to send</param>
    /// <returns></returns>
    public Task<Result> SendVerificationCodeAsync(string name, string email, string code);
}