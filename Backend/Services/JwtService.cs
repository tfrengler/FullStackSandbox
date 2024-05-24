using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FullStackSandbox.Services
{
    public class JwtService
    {
        private readonly string Key;
        private readonly TimeSpan ClockSkew;
        private readonly TimeSpan RefreshTokenExpireTime;
        private readonly TimeSpan AccessTokenExpireTime;
        private readonly ILogger<JwtService> Logger;

        public JwtService(IOptions<Models.SecurityConfig> configuration, ILogger<JwtService> logger)
        {
            Key = configuration.Value.JWTSecretSigningKey;
            ClockSkew = TimeSpan.FromSeconds(configuration.Value.JWTClockSkewInSeconds);
            RefreshTokenExpireTime = TimeSpan.FromMinutes(configuration.Value.JWTRefreshTokenExpiryInMinutes);
            AccessTokenExpireTime = TimeSpan.FromMinutes(configuration.Value.JWTAccessTokenExpiryInMinutes);
            Logger = logger;
        }

        public Models.JwtToken GenerateTokens(string username, IEnumerable<string> roles)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(username));
            Debug.Assert(roles is not null);
            Debug.Assert(!roles.Any(string.IsNullOrWhiteSpace));

            var TokenHandler = new JwtSecurityTokenHandler();
            byte[] TokenKeyAsBytes = Encoding.UTF8.GetBytes(Key);

            var BaseLineDate = DateTime.Now.ToUniversalTime();
            var AccessTokenExpiryTime = BaseLineDate.Add(AccessTokenExpireTime);
            var RefreshTokenExpiryTime = BaseLineDate.Add(RefreshTokenExpireTime);

            var Claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username)
            };

            foreach(string roleName in roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(Claims),
                Expires = AccessTokenExpiryTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(TokenKeyAsBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken AccessToken = TokenHandler.CreateToken(TokenDescriptor);
            string RefreshToken = GenerateRefreshToken();

            return new Models.JwtToken(TokenHandler.WriteToken(AccessToken), RefreshToken, RefreshTokenExpiryTime);
        }

        public string GenerateRefreshToken()
        {
            var TokenAsBytes = new byte[128 / 8];
            var RNGSecure = RandomNumberGenerator.Create();
            RNGSecure.GetBytes(TokenAsBytes);
            return Convert.ToBase64String(TokenAsBytes);
        }

        public bool ValidateTokenAndGetPrincipal(string token, bool ignoreExpired, [NotNullWhen(true)] out ClaimsPrincipal? validatedPrincipal)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));
            byte[] KeyAsBytes = Encoding.UTF8.GetBytes(Key);

            var TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = !ignoreExpired,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(KeyAsBytes),
                ClockSkew = ClockSkew
            };

            var TokenHandler = new JwtSecurityTokenHandler();
            SecurityToken ValidatedToken;

            try
            {
                validatedPrincipal = TokenHandler.ValidateToken(token, TokenValidationParameters, out ValidatedToken);
            }
            catch(Exception error) when (error is SecurityTokenException or ArgumentException or ArgumentNullException)
            {
                Logger.LogWarning("{message}", error.Message);
                validatedPrincipal = null;
                return false;
            }

            if (ValidatedToken is not JwtSecurityToken JWTSecurityToken)
            {
                Logger.LogWarning("Not a token?");
                validatedPrincipal = null;
                return false;
            }

            if (!JWTSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.LogWarning("Security algorithm mismatch: {algorithmName}", JWTSecurityToken.Header.Alg);
                validatedPrincipal = null;
                return false;
            }

            return true;
        }
    
        public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["xxx-token-expired"] = "1";
            }
            return Task.CompletedTask;
        }

        public static Task OnTokenValidated(TokenValidatedContext context)
        {
            context.Response.Headers["xxx-token-expired"] = "0";
            return Task.CompletedTask;
        }
    }
}