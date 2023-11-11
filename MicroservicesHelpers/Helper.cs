using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace MicroservicesHelpers
{
    public class Helper
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
    }
}
