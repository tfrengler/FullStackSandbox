using FullStackSandbox.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace FullStackSandbox.Services
{
    public sealed class UserService
    {
        private readonly byte[] Salt;
        private readonly ConcurrentDictionary<string, Models.UserRefreshToken> Sessions;
        private readonly Dictionary<string, Models.User> Users;

        public UserService(IOptions<Models.SecurityConfig> config)
        {
            string SaltAsString = config.Value.PasswordSalt;
            Salt = Encoding.UTF8.GetBytes(SaltAsString);

            Sessions = new();

            Users = new()
            {
                { "tfrengler", new Models.User("tfrengler", "jhIwsSdkWHiWW7LF9E5x0RmvM/k3QMzO", new string[] { "Normal", "Admin" }) }
            };
        }

        public void AddUserRefreshToken(string username, Models.UserRefreshToken refreshToken)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            Sessions[username] = refreshToken;
        }

        public bool RevokeUserRefreshToken(string username, string refreshToken)
        {
            if (!Sessions.TryGetValue(username, out var Token)) return false;
            if (Token.RefreshToken != refreshToken) return false;

            return Sessions.TryRemove(new KeyValuePair<string, Models.UserRefreshToken>(username, Token));
        }

        public bool ValidateAndGetRefreshToken(string username, string refreshToken, [NotNullWhen(returnValue: true)] out Models.UserRefreshToken? validatedToken)
        {
            if (!Sessions.TryGetValue(username, out validatedToken))
            {
                validatedToken = null;
                return false;
            }

            if (validatedToken.RefreshToken == refreshToken && DateTime.Now.ToUniversalTime() <= validatedToken.Expires)
            {
                return true;
            }

            validatedToken = null;
            return false;
        }

        public bool IsValidUser(Models.User user, [NotNullWhen(true)]out User? UserData)
        {
            if (!Users.TryGetValue(user.Username, out UserData)) return false;

            var HashedInputPassword = ComputeHash(user.Password);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(HashedInputPassword),
                Encoding.UTF8.GetBytes(UserData.Password)
            );
        }

        private string ComputeHash(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return string.Empty;

            byte[] InputAsBytes = Encoding.UTF8.GetBytes(rawData);
            var HashResultAsBytes = new Rfc2898DeriveBytes(InputAsBytes, Salt, 10000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(HashResultAsBytes.GetBytes(24));
        }
    }
}