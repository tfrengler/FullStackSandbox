using FullStackSandbox.Backend.Models;
using FullStackSandbox.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FullStackSandbox.Services
{
    public sealed class UserService
    {
        private readonly ConcurrentDictionary<string, Models.UserRefreshToken> Sessions;
        private readonly Dictionary<string, Models.User> Users;

        public UserService()
        {
            Sessions = new();

            Users = new()
            {
                { 
                    "tfrengler",
                    new Models.User(
                        "tfrengler",
                        HashedPassword.Create("tf499985", Encoding.ASCII.GetBytes("1234567890123456")),
                        new string[] { "Normal", "Admin" }
                    )
                }
            };
        }

        public void AddUserRefreshToken(string username, Models.UserRefreshToken refreshToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
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

        public bool IsValidUser(string username, string plainPassword, [NotNullWhen(true)]out User? UserData)
        {
            if (!Users.TryGetValue(username, out UserData))
            {
                return false;
            }

            var ExpectedPassword = UserData.Password;
            var UserGivenPassword = HashedPassword.Create(plainPassword, ExpectedPassword.SaltBytes);

            return ExpectedPassword.SecureEqualTo(UserGivenPassword);
        }

        /* NEW:
         * 1: Generate random bytes as salt (16 bytes)
         * 2: Generate hash bytes from password string with the salt (20 bytes)
         * 3: Store in separate (a) columns or single (b) column
         *      a: convert both byte arrays to base64 string
         *      b: combine both hash byte arrays to one, convert to base64
         *      
         * VERIFY:
         * 1: Depending on 3 above:
         *      a: retrieve both values from db, decode from base64 to byte arrays
         *      b: retrieve the value, decode from base64 to byte array, separate the salt and password part from the bytes
         * 2: Do 2 on the user supplied password with the salt from the db
         * 3: Compare both user supplied hash bytes with password hash bytes from db using FixedTimeEquals
        */
    }
}