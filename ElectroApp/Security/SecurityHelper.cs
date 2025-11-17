using System;
using System.Security.Cryptography;

namespace ElectroApp.Security
{
    public static class SecurityHelper
    {
        // Genera salt seguro
        public static byte[] GenerateSalt(int size = 16)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Genera hash PBKDF2 (SHA256) con iteraciones
        public static byte[] GenerateHash(string password, byte[] salt, int iterations = 100_000, int hashLength = 64)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(hashLength);
            }
        }
    }
}
