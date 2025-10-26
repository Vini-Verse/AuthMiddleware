using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMiddleware.Core
{
    public interface IPasswordHasher
    {
        void CreateHash(string password, out byte[] hash, out byte[] salt);
        bool VerifyHash(string password, byte[] storedHash, byte[] storedSalt);
    }
}
