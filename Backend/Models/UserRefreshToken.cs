using System;
using System.Diagnostics;

namespace FullStackSandbox.Models
{
    public sealed record UserRefreshToken
    {
        public UserRefreshToken(string refreshToken, DateTime expires)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(refreshToken));
            Debug.Assert(expires.Kind == DateTimeKind.Utc);

            Expires = expires;
            RefreshToken = refreshToken;
        }

        public DateTime Expires { get; set; }
        public string RefreshToken { get; set; }
    }
}