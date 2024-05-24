using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace FullStackSandbox.Backend.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly byte[] Salt;

        public TestController(IOptions<Models.SecurityConfig> config)
        {
            string SaltAsString = config.Value.PasswordSalt;
            Salt = Encoding.UTF8.GetBytes(SaltAsString);
        }

        [Route("generateHashedPassword")]
        [HttpGet]
        [AllowAnonymous]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> GenerateHashedPassword([FromQuery]string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return Problem(detail: "Query param 'password' is missing or empty", statusCode: StatusCodes.Status400BadRequest);
            }

            byte[] InputAsBytes = Encoding.UTF8.GetBytes(password);
            var HashResultAsBytes = new Rfc2898DeriveBytes(InputAsBytes, Salt, 10000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(HashResultAsBytes.GetBytes(24));
        }

        [Route("testAuth")]
        [HttpGet]
        [Authorize(Roles = "Normal,Admin")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> TestAuth()
        {
            return "All your base is belong to us";
        }
    }
}
