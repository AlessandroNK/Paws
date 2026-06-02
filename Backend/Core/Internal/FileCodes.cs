using System.Runtime.CompilerServices;
using Backend.Core.Services;

namespace Backend.Core.Internal;

/// <summary>
/// Provides short internal codes for source files and builds trace identifiers that include the file code.
/// </summary>
public static class FileCodes
{
        //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    private static readonly Dictionary<string, string> Codes = new()
    {
        { "ApiController.cs", "I01" },
        { "AppointmentsController.cs", "A01" },
        { "AppointmentsService.cs", "A02" },
        { "AppointmentsRepository.cs", "A03" },
        { "AppConfigService.cs", "C02" },
        { "AppConfigRepository.cs", "C03" },
        { "NotificationService.cs", "N02" },
        { "PetController.cs", "P01" },
        { "PetService.cs", "P02" },
        { "PetRepository.cs", "P03" },
        { "UserController.cs", "U01" },
        { "UserService.cs", "U02" },
        { "UserRepository.cs", "U03" },
        { "SecurityService.cs", "S02" },
    };


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
    /// <summary>
    /// Gets the internal code associated with a source file name.
    /// </summary>
    /// <param name="filePath">The full or relative path to the source file.</param>
    /// <returns>The configured file code, or <c>UNK</c> when no mapping exists.</returns>
    private static string GetCode(string filePath)
    {
        var fileName = System.IO.Path.GetFileName(filePath);
        return Codes.GetValueOrDefault(fileName, "UNK");
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Builds an internal trace code using the caller file mapping, caller line number, and a generated verification code
    /// (just to make people confuse).
    /// </summary>
    /// <param name="file">Caller source file path supplied automatically by <see cref="CallerFilePathAttribute"/>.</param>
    /// <param name="line">Caller source line number supplied automatically by <see cref="CallerLineNumberAttribute"/>.</param>
    /// <returns>A code in the format <c>{FILE}:{LINE}-{VERIFY}</c>, for example <c>FU03:0044-123456</c>.</returns>
    public static string CallerIC([CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        return $"{GetCode(file)}:{line:D4}-{SecurityService.GenerateVerificationCode()}";
    }
}