using System;
using System.Collections.Generic;

namespace FullStackSandbox.Models
{
    public sealed record User
    {
        public User() { }

        public User(string userName, string password, IEnumerable<string> roles)
        {
            ArgumentNullException.ThrowIfNull(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentNullException.ThrowIfNull(roles);

            Username = userName;
            Password = password;
            Roles = roles;
        }

        public User(string userName, string displayName, string password, IEnumerable<string> roles)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentNullException.ThrowIfNull(displayName);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentNullException.ThrowIfNull(roles);

            Username = userName;
            DisplayName = displayName;
            Password = password;
            Roles = roles;
        }

        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
    }
}