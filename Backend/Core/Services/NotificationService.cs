using System.Text;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Security;
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
/// <remarks>FN02</remarks>
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
    private Result<NotificationEnvironmentVariables> GetNotificationEnvironmentVariables()
    {
        var apiKey = Environment.GetEnvironmentVariable("MAIL_API_KEY");
        var senderEmail = Environment.GetEnvironmentVariable("MAIL_NO_REPLY_EMAIL");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
        {
            return new Result<NotificationEnvironmentVariables>
            {
                Success = false,
                Status = 500,
                Title = "Email service is misconfigured: One or more required environment variables are not set.",
                Code = "ENVIRONMENT_VARIABLES_NOT_SET",
                TraceCode = FileCodes.CallerIC(),
                Returnable = false
            };
        }

        var variables = new NotificationEnvironmentVariables
        {
            ApiKey = apiKey,
            SenderEmail = senderEmail
        };

        return new Result<NotificationEnvironmentVariables>
        {
            Success = true,
            Status = 200,
            Title = "Environment variables retrieved successfully.",
            Code = "SUCCESS",
            TraceCode = FileCodes.CallerIC(),
            Returnable = false,
            Data = variables
        };
    }

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
            var variables = GetNotificationEnvironmentVariables();

            if (!variables.Success || variables.Data is null) return variables;

            var apiKey = variables.Data.ApiKey;
            var senderEmail = variables.Data.SenderEmail;

            // Extract first name from full name
            var firstName = name.Split(' ').FirstOrDefault() ?? "Usuario";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            // Prepare the email content
            var requestBody = new
            {
                sender = new { email = senderEmail, name = "🐾Paws acá🐶" },
                to = new[] { new { email, name = email } },
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
                    Title = "Verification code sent successfully.",
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
                Title = "Failed to send verification code.",
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
                Title = "An error occurred while sending the verification code.",
                Code = "VERIFICATION_CODE_SEND_FAILED",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sends a login code to the given email address.
    /// </summary>
    /// <param name="name">The name of the user</param>
    /// <param name="email">The email to send the code to</param>
    /// <param name="code">The code to send</param>
    /// <returns></returns>
    public async Task<Result> SendLoginCodeAsync(string name, string email, string code)
    {
        try
        {
            // Get API key and sender email from environment variables
            var variables = GetNotificationEnvironmentVariables();

            if (!variables.Success || variables.Data is null) return variables;

            var apiKey = variables.Data.ApiKey;
            var senderEmail = variables.Data.SenderEmail;

            // Extract first name from full name
            var firstName = name.Split(' ').FirstOrDefault() ?? "Usuario";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            // Prepare the email content
            var requestBody = new
            {
                sender = new { email = senderEmail, name = "🐾Paws acá🐶" },
                to = new[] { new { email, name = email } },
                subject = $"Tu código para Iniciar Sesión en Paws es {code}",
                htmlContent = $@"
                            <h2>¡Hola!</h2>
                            <p>{firstName}, aquí tienes tu <b>código de un solo uso</b> para iniciar sesión en <strong>Paws</strong>:</p>
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
                LogHelpers.LogInfo(_logger, $"Login code sent to {email}");
                return new Result
                {
                    Success = true,
                    Status = 200,
                    Title = "Login code sent successfully.",
                    Code = "LOGIN_CODE_SENT",
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
                Title = "Failed to send verification code.",
                Code = "LOGIN_CODE_NOT_SENT",
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
                Title = "An error occurred while sending the login code.",
                Code = "LOGIN_CODE_NOT_SEND_FAILED",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sends an ownership share code to the given email address.
    /// </summary>
    /// <param name="invitation">The <see cref="OwnershipInvitation"/> to send</param>
    /// <returns></returns>
    public async Task<Result> SendOwnershipInvitationLink(OwnershipInvitation invitation)
    {
        try
        {
            // Get API key and sender email from environment variables
            var variables = GetNotificationEnvironmentVariables();

            if (!variables.Success || variables.Data is null) return variables;

            var apiKey = variables.Data.ApiKey;
            var senderEmail = variables.Data.SenderEmail;

            // Client
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var pet = invitation.Pet;
            var user = invitation.User;
            if (pet is null || user is null)
            {
                return new Result
                {
                    Success = false,
                    Status = 400,
                    Title = "Invalid ownership invitation.",
                    Code = "INVALID_OWNERSHIP_INVITATION",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            var accountP = invitation.NewOwnerHasAccount
                ? $"Sabemos que eres cliente PAWS, por eso te facilitamos la vida, solo <a href='{invitation.OwnershipLink}' style='color:#f05a22; text-decoration:none;'>ingresa a este enlace</a> para continuar."
                : $"Bienvenido a Paws! Para comenzar, ingresa a <a href='{invitation.OwnershipLink}' style='color:#f05a22; text-decoration:none;'>nuestro sitio web y crea tu cuenta</a>.";

            var emoji = pet.Species switch
            {
                PetSpecies.Dog => "🐶",
                PetSpecies.Cat => "🐱",
                PetSpecies.Bunny => "🐰",
                PetSpecies.Hamster => "🐹",
                PetSpecies.Turtle => "🐢",
                PetSpecies.Cow => "🐮",
                PetSpecies.Horse => "🐴",
                PetSpecies.Bird => "🐦",
                PetSpecies.Fish => "🐟",
                PetSpecies.Reptile => "🦎",
                PetSpecies.Rodent => "🐭",
                _ => "🐾"
            };
            // Prepare the email content
            var requestBody = new
            {
                sender = new { email = senderEmail, name = "🐾Paws acá🐶" },
                to = new[] { new { email = invitation.NewOwnerEmail, name = invitation.NewOwnerName } },
                subject = $"{invitation.Pet.Name} {emoji} te pide que seas su dueño en Paws",
                htmlContent = $@"
                    <h2>¡Hola {invitation.NewOwnerName}!</h2>
                    <p>{user.Name} quiere compartir contigo la propiedad de su mascota <strong>{pet.Name}</strong> en <b>Paws</b>.</p>
                    <p>Para aceptar la invitación y convertirte en co‑propietario, dale click al siguiente <b>enlace de un solo uso</b>:</p>
                    <p style='font-size: 20px; font-weight: 600; color: #f05a22;'><a href='{invitation.OwnershipLink}' style='color:#f05a22; text-decoration:none;'>Aceptar invitación</a></p>
                    <p>Este código es válido por 24 horas.</p>
                    {accountP}
                    <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                    <p><em>Paws es un proyecto académico, hecho con fines de aprendizaje.</em></p>
                    <p><a href='' style='color:#f05a22; text-decoration:none;'>Pronto tendremos sitio web oficial</a></p>"
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (response.IsSuccessStatusCode)
            {
                LogHelpers.LogInfo(_logger, $"Verification code sent to {invitation.NewOwnerEmail}");
                return new Result
                {
                    Success = true,
                    Status = 200,
                    Title = "Verification code sent successfully.",
                    Code = "VERIFICATION_CODE_SENT",
                    TraceCode = FileCodes.CallerIC(),
                    Returnable = true
                };
            }

            LogHelpers.LogError(
                _logger,
                $"Failed to send verification code to {invitation.NewOwnerEmail}. Status: {response.StatusCode} Error: {response.ReasonPhrase} {response.Content.ReadAsStringAsync().Result}"
            );
            return new Result
            {
                Success = false,
                Status = (int)response.StatusCode,
                Title = "Failed to send verification code.",
                Code = "VERIFICATION_CODE_NOT_SENT",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending ownership share code to {Email}", invitation.NewOwnerEmail);
            return new Result
            {
                Success = false,
                Status = 500,
                Title = "An error occurred while sending the ownership share code.",
                Code = "OWNERSHIP_SHARE_CODE_SEND_FAILED",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
        }
    }
}