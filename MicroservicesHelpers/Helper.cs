using MicroservicesHelpers.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroservicesHelpers
{
    public static class Helper
    {
        /// <summary>
        /// This method is used to generate a random salt
        /// </summary>
        /// <returns></returns>
        public static string GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }


        /// <summary>
        /// This method is used to hash a password
        /// </summary>
        /// <param name="password"> Thhe password to hash</param>
        /// <param name="salt"> The salt to use</param>
        /// <returns> The hashed password</returns>
        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] hashedBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32);

            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// This method is used to Encrypt a string
        /// </summary>
        /// <param name="plainText"> The string to encrypt</param>
        /// <param name="key"> The key to use</param>
        /// <returns> The encrypted string</returns>
        public static string EncryptString(string plainText, string key)
        {
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// This method is used to decrypt a string
        /// </summary>
        /// <param name="cipherText"> The string to decrypt</param>
        /// <param name="key"> The key to use</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(buffer, 0, buffer.Length);
                        cs.FlushFinalBlock();
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Validates an email address using the System.Net.Mail.MailAddress class.
        /// </summary>
        /// <param name="email">The email address to be validated.</param>
        /// <returns>Returns true if the email address is valid; otherwise, returns false.</returns>
        public static bool ValidateEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                // The MailAddress constructor throws FormatException if the email is not valid
                return false;
            }
        }

        /// <summary>
        /// Validates a password using a regular expression pattern.
        /// </summary>
        /// <param name="password">The password to be validated.</param>
        /// <returns>Returns true if the password is valid; otherwise, returns false.</returns>
        public static bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                // Password should not be null, empty, or contain only whitespaces
                return false;
            }

            var passwordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\\W_])[A-Za-z0-9\\?\\W_]{8,}$");

            return passwordRegex.IsMatch(password);
        }

        /// <summary>
        /// Sends an email using the provided mail settings and parameters.
        /// </summary>
        /// <param name="settings">The mail settings, including host, port, mail address, and password.</param>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email. If null, a default subject is used.</param>
        /// <param name="body">The body content of the email.</param>
        public static void SendEmail(MailSettings settings, string to, string subject, string body)
        {
            // Create an SMTP client with the specified host, port, and credentials
            SmtpClient smtpClient = new SmtpClient(settings.Host);
            smtpClient.Port = settings.Port;
            smtpClient.Credentials = new NetworkCredential(settings.Mail, settings.Password);
            smtpClient.EnableSsl = true;

            // Create the email message with the sender's mail address and the recipient's email address
            MailMessage mailMessage = new MailMessage(settings.Mail, to);

            // Set the subject and body of the email, using default values if not provided
            mailMessage.Subject = subject != null ? subject : "Test Email";
            mailMessage.Body = body != null ? body : "This is a test email.";

            try
            {
                // Attempt to send the email
                smtpClient.Send(mailMessage);
            }
            catch
            {
                // Propagate any exception that occurs during the email sending process
                throw;
            }
        }

        /// <summary>
        /// Generates a random recovery code of a specified length using alphanumeric characters.
        /// </summary>
        /// <returns>Returns a string representing the generated recovery code.</returns>
        public static string GenerateRecoveryCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int codeLength = 8;

            // Using Random to generate a random code
            Random random = new Random();

            // Create a string by selecting random characters from the specified character set
            string recoveryCode = new string(Enumerable.Repeat(chars, codeLength)
                                          .Select(s => s[random.Next(s.Length)]).ToArray());

            // Make sure to store this code for later verification
            return recoveryCode;
        }

        /// <summary>
        /// Gets the description associated with an enum value, if available.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The description of the enum value, or the enum value's string representation if no description is available.</returns>
        public static string GetDescription(this Enum value)
        {
            try
            {
                // Get the field information for the enum value
                var field = value.GetType().GetField(value.ToString());

                // Get the DescriptionAttribute associated with the enum value, if any
                var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                // Return the description if available, otherwise return the enum value's string representation
                return attribute == null ? value.ToString() : attribute.Description;
            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., log it, rethrow it, or return a default value)
                throw new Exception($"An error occurred while getting the description: {ex.Message}");
            }
        }


    }
}
