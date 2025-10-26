using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMiddleware.models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public List <RefreshToken> RefreshTokens { get; set; } = new();
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
