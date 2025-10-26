using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMiddleware.models
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Revoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string CreateByIp { get; set; } = string.Empty;
        public string RevokedByIp { get; set; } = string.Empty;
        public string ReplacedByToken { get; set; } = string.Empty;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
