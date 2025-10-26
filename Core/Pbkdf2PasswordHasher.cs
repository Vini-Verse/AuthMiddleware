using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMiddleware.Core
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        public void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            salt = new byte[16];
            rng.GetBytes(salt);
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            hash = pbkdf2.GetBytes(32);
        }

        public bool VerifyHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, storedSalt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(32);
            return computedHash.SequenceEqual(storedHash);
        }
    }
}
