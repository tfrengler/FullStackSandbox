namespace FullStackSandbox.Models.ResponseObjects
{
    public sealed record AuthenticateRequestObject
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}