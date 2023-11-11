
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace MicroservicesHelpers;
public class RSAHandler
{
    public static string GetPublicKey(string publicKey, string salt)
    {
        string key = Helper.DecryptString(publicKey, salt)
            .Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");

        RSA rsa = RSA.Create();

        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key), out _);

        return rsa.ToXmlString(false);
    }

    public static string GetPrivateKey(string privateKey, string salt)
    {
        RSA rsa = RSA.Create();

        rsa.ImportRSAPrivateKey(NoName(privateKey, salt), out _);

        return rsa.ToXmlString(true);
    }

    /// <summary>
    /// Decrypts the provided key and extracts the content between the RSA key start and end tags.
    /// </summary>
    /// <param name="key">The encrypted key to be decrypted.</param>
    /// <returns>The decrypted data extracted from the RSA key.</returns>
    private static byte[] NoName(string key, string salt)
    {
        // Decrypts the key using the DecryptString method from the Helper, using the salt (_salt).
        key = Helper.DecryptString(key, salt);

        // Defines the pattern to extract content between the start and end tags of the RSA key.
        string keyPattern = @"-----BEGIN (RSA PRIVATE|PUBLIC) KEY-----\s*(.*?)\s*-----END \1 KEY-----";

        // Replaces the encrypted key with the content extracted between the start and end tags.
        string keyContent = Regex.Replace(key, keyPattern, "$2", RegexOptions.Singleline);

        // Converts the extracted content (in base64 format) to a byte array.
        return Convert.FromBase64String(@keyContent);
    }

}
