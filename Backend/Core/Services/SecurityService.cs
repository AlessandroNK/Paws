using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Core.Internal;
using Backend.Core.Models.Result;

namespace Backend.Core.Services;

public partial class SecurityService
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------

    #region Regex

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex(@"[Kk]")]
    private static partial Regex KLetterRegex();

    [GeneratedRegex(@"[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[!@#$%^&*(),.?"":{}|<>\/ _-]")]
    private static partial Regex SpecialCharRegex();

    #endregion


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
    /// Creates a Hash from a string.
    /// </summary>
    /// <param name="toEncrypt">The string to encrypt</param>
    /// <returns></returns>
    public static Result<string> HashWithSalt(string toEncrypt)
    {
        // Global key
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

        // Validations
        if (string.IsNullOrWhiteSpace(toEncrypt))
            return new Result<string>
            {
                Success = false,
                Code = "EMPTY_STRING",
                Status = 400,
                Message = "The string to encrypt cannot be empty",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        if (string.IsNullOrEmpty(key))
            return new Result<string>
            {
                Success = false,
                Code = "NO_ENCRYPTION_KEY",
                Status = 500,
                Message = "No encryption key found",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        using (var sha = SHA256.Create())
        {
            // Combine salt + input
            var combined = Encoding.UTF8.GetBytes(key + toEncrypt);
            var hash = sha.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Pues eso, encripta un string.
    /// </summary>
    /// <param name="toEncrypt">The string to encrypt</param>
    /// <returns></returns>
    public static Result<string> EncryptString(string toEncrypt)
    {
        // Global key
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

        // Validations
        if (string.IsNullOrWhiteSpace(toEncrypt))
            return new Result<string>
            {
                Success = false,
                Code = "EMPTY_STRING",
                Status = 400,
                Message = "The string to encrypt cannot be empty",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        if (string.IsNullOrEmpty(key))
            return new Result<string>
            {
                Success = false,
                Code = "NO_ENCRYPTION_KEY",
                Status = 500,
                Message = "No encryption key found",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        // Encrypt the string using AES encryption
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Random IV so this is cooler
            aes.GenerateIV();
            var iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(toEncrypt);
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine IV + cipher bytes for transmission/storage
                byte[] finalString = new byte[iv.Length + cipherBytes.Length];
                Buffer.BlockCopy(iv, 0, finalString, 0, iv.Length);
                Buffer.BlockCopy(cipherBytes, 0, finalString, iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(finalString);
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Pues eso, desencripta un string previamente encriptado con EncryptString.
    /// </summary>
    /// <param name="toDecrypt"></param>
    /// <returns></returns>
    public static Result<string> DecryptString(string toDecrypt)
    {
        // Global key
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

        // Validations
        if (string.IsNullOrWhiteSpace(toDecrypt))
            return new Result<string>
            {
                Success = false,
                Code = "EMPTY_STRING",
                Status = 400,
                Message = "The string to decrypt cannot be empty",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        if (string.IsNullOrEmpty(key))
            return new Result<string>
            {
                Success = false,
                Code = "NO_ENCRYPTION_KEY",
                Status = 500,
                Message = "No encryption key found",
                IC = FileCodes.CallerIC(),
                Returnable = false
            };

        byte[] combinedBytes = Convert.FromBase64String(toDecrypt);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning of the combined bytes
            byte[] iv = new byte[aes.BlockSize / 8];
            Buffer.BlockCopy(combinedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            {
                byte[] cipherBytes = new byte[combinedBytes.Length - iv.Length];
                Buffer.BlockCopy(combinedBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks the validity of a password. A valid password must be at least 8 characters long, cannot contain spaces,
    /// must contain at least one lowercase letter, one uppercase letter, one number, one special character, and one 'K'
    /// letter.
    /// </summary>
    /// <param name="password">The password to check</param>
    /// <returns>It returns a <see cref="Result"/></returns>
    public static Result IsPasswordValid(string password)
    {
        var isValid = true;
        var result = new Result
        {
            Success = true,
            Code = "VALID_PASSWORD",
            Status = 200,
            Message = "Password is valid",
            IC = FileCodes.CallerIC(),
            Returnable = true
        };

        if (password.Length < 8)
        {
            isValid = false;
            result.AddError(
                "PASSWORD_TOO_SHORT",
                ["Password must be at least 8 characters long"]
            );
        }

        if (password.Contains(' '))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_CONTAINS_SPACES",
                ["Password cannot contain spaces"]
            );
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_NO_LOWERCASE",
                ["Password must contain at least one lowercase letter"]
            );
        }

        if (!UppercaseRegex().IsMatch(password))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_NO_UPPERCASE",
                ["Password must contain at least one uppercase letter"]
            );
        }

        if (!DigitRegex().IsMatch(password))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_NO_DIGIT",
                ["Password must contain at least one number"]
            );
        }

        if (!SpecialCharRegex().IsMatch(password))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_NO_SPECIAL_CHAR",
                ["Password must contain at least one special character"]
            );
        }

        if (!KLetterRegex().IsMatch(password))
        {
            isValid = false;
            result.AddError(
                "PASSWORD_NO_K_LETTER",
                ["Password must contain at least one 'K' letter"]
            );
        }

        return isValid
            ? result
            : new Result
            {
                Success = false,
                Code = "INVALID_PASSWORD",
                Status = 400,
                Message = "Password is invalid",
                IC = FileCodes.CallerIC(),
                Returnable = true,
                Errors = result.Errors
            };
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Generates a random verification code of 6 characters. The code can contain uppercase letters and numbers. It is
    /// used to verify the user's email when signing up or to reset the user's password. It is also used to verify the
    /// user's identity when signing in from a new device. It is generated using a random number generator and a combination
    /// of uppercase letters and numbers.
    /// </summary>
    /// <returns></returns>
    public static string GenerateVerificationCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Performs some verifications on the device Id for security purposes
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public static Result ValidateDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return new Result
            {
                Success = false,
                Code = "MISSING_DEVICE_ID",
                Status = 400,
                Message = "Device id is required",
                IC = FileCodes.CallerIC(),
                Returnable = true
            };

        // Maybe I will add more validations but today, I'm tired so...
        return new Result
        {
            Success = true,
            Code = "VALID_DEVICE_ID",
            Status = 200,
            Message = "Device id is valid",
            IC = FileCodes.CallerIC(),
            Returnable = true
        };
    }
}