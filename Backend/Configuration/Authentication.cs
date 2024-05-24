using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.Text;

namespace FullStackSandbox.Configuration
{
    public static class Authentication
    {
        public static void ConfigureAuthentication(this IServiceCollection self, Models.SecurityConfig config)
        {
            Debug.Assert(config is not null);

            self.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                byte[] Key = Encoding.ASCII.GetBytes(config.JWTSecretSigningKey);
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = false, // on production make it true
                    ValidateAudience = false, // on production make it true
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ClockSkew = TimeSpan.FromSeconds(config.JWTClockSkewInSeconds)
                };
                options.Events = new JwtBearerEvents {
                    OnAuthenticationFailed = FullStackSandbox.Services.JwtService.OnAuthenticationFailed,
                    OnTokenValidated = FullStackSandbox.Services.JwtService.OnTokenValidated,
                };
            });
        }
    }
}
