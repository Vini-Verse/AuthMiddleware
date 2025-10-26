using AuthMiddleware.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


namespace AuthMiddleware.Integration.ASP.NET
{
    public static class MyAuthServiceCollectionExtensions
    {
        public static IServiceCollection AddMyAuth(this IServiceCollection services, IConfiguration config)
        {
            var jwtOptions = new JwtOptions();
            config.Bind("Jwt", jwtOptions);
            services.AddSingleton(jwtOptions);

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtFactory, JwtFactory>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
