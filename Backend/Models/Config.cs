using System;

namespace FullStackSandbox.Models
{
    public sealed record SystemConfig
    {
        public string LogsDir { get; init; } = string.Empty;
        public string DatabaseFile { get; init; } = string.Empty;

        public override string ToString()
        {
            var Output = new string[]
            {
                $"-| DatabaseFile   : {DatabaseFile}",
                $"-| LogsDir        : {LogsDir}"
            };

            return string.Join(System.Environment.NewLine, Output);
        }
    }

    public sealed record SecurityConfig
    {
        public string JWTSecretSigningKey { get; init; } = string.Empty;
        public uint JWTClockSkewInSeconds { get; init; }
        public uint JWTAccessTokenExpiryInMinutes { get; init; }
        public uint JWTRefreshTokenExpiryInMinutes { get; init; }
        public string PasswordSalt { get; init; } = string.Empty;
        public string[] CORSOrigins { get; init; } = Array.Empty<string>();
        public string[] CORSAllowedMethods { get; init; } = Array.Empty<string>();

        public override string ToString()
        {
            var Output = new string[]
            {
                $"-| JWTSecretSigningKey            : {JWTSecretSigningKey}",
                $"-| JWTClockSkewInSeconds          : {JWTClockSkewInSeconds}",
                $"-| PasswordSalt                   : {PasswordSalt}",
                $"-| JWTAccessTokenExpiryInMinutes  : {JWTAccessTokenExpiryInMinutes}",
                $"-| JWTRefreshTokenExpiryInMinutes : {JWTRefreshTokenExpiryInMinutes}",
                $"-| CORSOrigins                    : {string.Join(';', CORSOrigins)}",
                $"-| CORSAllowedMethods             : {string.Join(';', CORSAllowedMethods)}"
            };

            return string.Join(System.Environment.NewLine, Output);
        }
    }
}