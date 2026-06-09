using System.Text.Json;
using Backend.Core.Models.Intern;
using Backend.Core.Services;

namespace Backend.Core.Internal;

/// <summary>
/// A class that does things (I'm tired, don't judge me for this documentation)
/// </summary>
public static class Env
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This one is easy to guess don't mames
    /// </summary>
    private static Version Version { get; } = new Version(0, 0, 1);

    private static string InteractionCode { get; set; }

    private static readonly TimeZoneInfo ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");


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
    /// Returns the version of the application as a string.
    /// </summary>
    /// <returns></returns>
    public static string GetVersion() => Version.ToString();

    // -----------------------------------------------------------------------------------------------------------------
    public static string GetAppContext() => "No hay contexto por ahora, I am wrking on it";

    // -----------------------------------------------------------------------------------------------------------------
    public static string GetInteractionCode() => InteractionCode;

    // -----------------------------------------------------------------------------------------------------------------
    public static void SetInteractionCode(string deviceId)
    {
        InteractionCode = $"{deviceId}-{Guid.NewGuid()}-{DateTime.Now}";
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static string GetProjectContext()
    {
        var context = new ProjectContext
        {
            Name = "PAWS",
            Type = "Veterinary Appointment Management Platform",
            Purpose = "Automates and optimizes veterinary appointment scheduling and clinic operations.",
            Summary =
                "PAWS® is a veterinary appointment management platform designed to automate and optimize scheduling processes for veterinary clinics. Its primary purpose is to improve the experience of pet owners while increasing operational efficiency for clinic staff.",

            Users =
            [
                "Pet Owners",
                "Veterinarians",
                "Administrative Staff",
                "Clinic Managers"
            ],

            Features =
            [
                "Appointment Management",
                "Pet Management",
                "Medical History Tracking",
                "Role-Based Access Control",
                "Notifications and Reminders",
                "LuckyTime Waitlist System",
                "InsightVet Analytics Dashboard",
                "Audit Logging",
                "Authentication and Security"
            ],

            Goals =
            [
                "Improve customer experience",
                "Reduce scheduling conflicts",
                "Automate clinic workflows",
                "Increase operational visibility",
                "Protect sensitive data"
            ],

            Limitations =
            [
                "Single clinic deployment",
                "No online payments",
                "No telemedicine",
                "No insurance integrations",
                "No multi-branch support"
            ]
        };

        return JsonSerializer.Serialize(
            context,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static TimeZoneInfo GetClinicTimeZone() => ClinicTimeZone;

    // -----------------------------------------------------------------------------------------------------------------
    public static DateTime ToClinicLocal(DateTime utcTime)
    {
        var utc = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, ClinicTimeZone);
    }
}