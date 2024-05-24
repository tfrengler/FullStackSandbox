using FullStackSandbox.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FullStackSandbox.Backend.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : Controller
    {
        public TestController()
        {

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

            return HashedPassword.Create(password).AsBase64String();
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
