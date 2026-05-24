using System.Runtime.CompilerServices;
using Backend.Core.Services;

namespace Backend.Core.Internal;

public class FileCodes
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    private static readonly Dictionary<string, string> Codes = new()
    {
        { "UserRepository.cs", "FU01" },
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


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    public static string GetCode(string filePath)
    {
        var fileName = System.IO.Path.GetFileName(filePath);
        return Codes.TryGetValue(fileName, out var code) ? code : "UNK";
    }

    // -----------------------------------------------------------------------------------------------------------------
    public static string CallerIC([CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        return $"{GetCode(file)}:{line:D4}-{SecurityService.GenerateVerificationCode()}";
    }
}