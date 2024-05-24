namespace FullStackSandbox.Models.RequestObjects
{
    public sealed record AuthenticationRequestObject
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}