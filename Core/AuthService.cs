using AuthMiddleware.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AuthMiddleware.Core
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IJwtFactory _jwt;
        private readonly IPasswordHasher _hasher;
        private readonly JwtOptions _options;

        public AuthService(IAuthRepository repo, IJwtFactory jwt, IPasswordHasher hasher, JwtOptions options)
        {
            _repo = repo;
            _jwt = jwt;
            _hasher = hasher;
            _options = options;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            if(await _repo.GetByEmailAsync(request.Email) is not null)
                throw new InvalidOperationException("Email already registered");

            _hasher.CreateHash(request.Password, out var hash, out var salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
            };

            user.RefreshTokens.Add(new RefreshToken());

            await _repo.AddUserAsync(user);
            await _repo.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(user);
            var activeRefresh = user.RefreshTokens.Last();

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = activeRefresh.Token,
                ExpiresAt = activeRefresh.ExpiresAt,
                User = new UserDto { Id = user.Id, Email = user.Email }
            };
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email)
                ?? throw new InvalidOperationException("Invalid email or password");
            if(!_hasher.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new InvalidOperationException("Invalid email or password");

            var refresh = CreateRefreshToken();
            user.RefreshTokens.Add(refresh);
            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(user);

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.ExpiresAt,
                User = new UserDto { Id = user.Id, Email = user.Email }
            };
        }

        public async Task<AuthResult> RefreshAsync(string token)
        {
            var user = await _repo.GetByRefreshTokenAsync(token)
                ?? throw new SecurityException("Invalid token");

            var existing = user.RefreshTokens.Single(x => x.Token == token);

            if (!existing.IsActive)
                throw new SecurityException("Token expired or revoked");

            var newRefreshToken = CreateRefreshToken();
            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(user);
            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = newRefreshToken.ExpiresAt,
                User = new UserDto { Id = user.Id, Email = user.Email }
            };
        }

        public async Task RevokeAsync(string token)
        {
            var user = await _repo.GetByRefreshTokenAsync(token);
            if (user == null) return;

            var refresh = user.RefreshTokens.SingleOrDefault(x => x.Token == token);
            if (refresh == null || !refresh.IsActive) return;
            
            refresh.RevokedAt = DateTime.UtcNow;
            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();
        }

        private RefreshToken CreateRefreshToken() =>
            new()
            {
                Token = _jwt.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow,
            };
    }
}
