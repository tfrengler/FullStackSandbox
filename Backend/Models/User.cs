using FullStackSandbox.Backend.Models;
using System;
using System.Collections.Generic;

namespace FullStackSandbox.Models
{
    public sealed record User
    {
        public User() { }

        public User(string userName, HashedPassword password, IEnumerable<string> roles)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(roles);

            Username = userName;
            Password = password;
            Roles = roles;
        }

        public User(string userName, string displayName, HashedPassword password, IEnumerable<string> roles)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentNullException.ThrowIfNull(displayName);
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(roles);

            Username = userName;
            DisplayName = displayName;
            Password = password;
            Roles = roles;
        }

        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public HashedPassword Password { get; init; }
        public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
    }
}