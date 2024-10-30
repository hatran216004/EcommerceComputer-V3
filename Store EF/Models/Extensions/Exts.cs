using System;
using System.Security.Cryptography;
using System.Text;

namespace Store_EF.Models.Extensions
{
    public static class Exts
    {
        static byte[] GenerateSalt(int size = 16)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }

        public static string GenUniqueWithSalt(this string email)
        {
            byte[] salt = GenerateSalt();
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(email);
                byte[] saltedInput = new byte[inputBytes.Length + salt.Length];

                Buffer.BlockCopy(salt, 0, saltedInput, 0, salt.Length);
                Buffer.BlockCopy(inputBytes, 0, saltedInput, salt.Length, inputBytes.Length);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in sha256.ComputeHash(saltedInput))
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}