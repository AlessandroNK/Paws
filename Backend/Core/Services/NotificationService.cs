using System.Text;
using Backend.Core.Internal;
using Backend.Core.Models.Result;
using Backend.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Backend.Core.Services;

/// <summary>
/// The service to handle notifications.
/// This service is responsible for managing notifications in the application.
/// It provides methods for creating, updating, deleting, and retrieving notifications.
/// The implementation of this interface should handle the data access logic, such as interacting with a database
/// or any other data source.
/// </summary>
public class NotificationService(
    ILogger<NotificationService> logger
) : INotificationService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The logger to log messages.
    /// </summary>
    private readonly ILogger<NotificationService> _logger = logger;


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
    public async Task<Result> SendVerificationCodeAsync(string name, string email, string code)
    {
        try
        {
            // Get API key and sender email from environment variables
            var apiKey = Environment.GetEnvironmentVariable("MAIL_API_KEY");
            var senderEmail = Environment.GetEnvironmentVariable("MAIL_NO_REPLY_EMAIL");
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
                return new Result
                {
                    Success = false,
                    Status = 500,
                    Message =
                        "Email service is misscofigured configured: MAIL_API_KEY environment variable is not set.",
                    Code = "VERIFICATION_CODE_NOT_SENT",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = false
                };

            // Extract first name from full name
            var firstName = name.Split(' ').FirstOrDefault() ?? "Usuario";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            // Prepare the email content
            var requestBody = new
            {
                sender = new { email = senderEmail, name = "Paws app" },
                to = new[] { new { email = email, name = email } },
                subject = $"Tu código para Registro en Paws es {code}",
                htmlContent = $@"
                            <h2>¡Hola!</h2>
                            <p>{firstName}, aquí tienes tu <b>código de un solo uso</b> para verificar tu cuenta en <strong>Paws</strong>:</p>
                            <p style='font-size: 20px; font-weight: 500; color: #f05a22;'>{code}</p>
                            <p>Si no solicitaste este código, simplemente ignora este mensaje. No pasa nada 🙂</p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                            <p><em>Paws es un proyecto académico, hecho con fines de aprendizaje.</em></p>
                            <p><a href='https://' style='color:#f05a22; text-decoration:none;'>no tengo web para Paws aún</a></p>>"
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (response.IsSuccessStatusCode)
            {
                LogHelpers.LogInfo(_logger, $"Verification code sent to {email}");
                return new Result
                {
                    Success = true,
                    Status = 200,
                    Message = "Verification code sent successfully.",
                    Code = "VERIFICATION_CODE_SENT",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            LogHelpers.LogError(
                _logger,
                $"Failed to send verification code to {email}. Status: {response.StatusCode} Error: {response.ReasonPhrase} {response.Content.ReadAsStringAsync().Result}"
            );
            return new Result
            {
                Success = false,
                Status = (int)response.StatusCode,
                Message = "Failed to send verification code.",
                Code = "VERIFICATION_CODE_NOT_SENT",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending verification code to {Email}", email);
            return new Result
            {
                Success = false,
                Status = 500,
                Message = "An error occurred while sending the verification code.",
                Code = "VERIFICATION_CODE_SEND_FAILED",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}