namespace FullStackSandbox.Models.ResponseObjects
{
    public sealed record TokenResponseObject
    {
        public System.DateTime Expires { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}