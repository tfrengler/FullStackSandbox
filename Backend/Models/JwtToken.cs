using System;
using System.Diagnostics;

namespace FullStackSandbox.Models
{
    public sealed record JwtToken
    {
        public JwtToken() { }

        public JwtToken(string access, string refresh, DateTime expires)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(access));
            Debug.Assert(!string.IsNullOrWhiteSpace(refresh));
            Debug.Assert(expires.Kind == DateTimeKind.Utc);

            Access = access;
            Refresh = refresh;
            Expires = expires;
        }

        public string Access { get; init; } = string.Empty;
        public string Refresh { get; init; } = string.Empty;
        public DateTime Expires { get; }
    }
}