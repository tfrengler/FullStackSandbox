namespace FullStackSandbox.Models.RequestObjects
{
    public sealed record TokenRequestObject
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}