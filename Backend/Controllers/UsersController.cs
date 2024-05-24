using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using FullStackSandbox.Services;
using System.Diagnostics;
using FullStackSandbox.Models;
using FullStackSandbox.Models.RequestObjects;
using FullStackSandbox.Models.ResponseObjects;

namespace FullStackSandbox.Controllers
{
    [Route("session")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JwtService JWTService;
        private readonly UserService Users;
        private readonly ILogger<UsersController> Logger;

        public UsersController(JwtService JWTService, UserService userServiceRepository, ILogger<UsersController> logger)
        {
            this.JWTService = JWTService;
            Users = userServiceRepository;
            Logger = logger;
        }

        /// <summary>
        /// Authenticates a user in the system and returns a set of access and refresh tokens.
        /// </summary>
        /// <response code="200">Returns the new access and refresh token set.</response>
        /// <response code="401">If the provided user credentials are incorrect.</response>
        /// <response code="400">If the request is malformed and/or failed validation criteria.</response>
        [Route("authenticate")]
        [HttpPost]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TokenResponseObject> Authenticate(AuthenticationRequestObject requestObject)
        {
            if (string.IsNullOrWhiteSpace(requestObject.Username) || string.IsNullOrWhiteSpace(requestObject.Password))
            {
                return Problem(detail: "Username or password is empty or missing", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!Users.IsValidUser(new User(requestObject.Username, requestObject.Password, Array.Empty<string>()), out User? ValidUser))
            {
                Logger.LogWarning($"Authentication request failed due to wrong credentials: {requestObject.Username}");
                return Unauthorized();
            }

            JwtToken Tokens = JWTService.GenerateTokens(ValidUser.Username, ValidUser.Roles);

            Users.AddUserRefreshToken(ValidUser.Username, new UserRefreshToken(Tokens.Refresh, Tokens.Expires));
            Logger.LogInformation($"Authentication succeeded for user {ValidUser.Username} (expires: {Tokens.Expires})");

            return Ok(new TokenResponseObject()
            {
                Expires = Tokens.Expires,
                AccessToken = Tokens.Access,
                RefreshToken = Tokens.Refresh
            });
        }

        /// <summary>
        /// Refreshes an existing set of tokens using the refresh token.
        /// </summary>
        /// <response code="200">Returns the new access and refresh token set.</response>
        /// <response code="401">If the access token is invalid or unknown.</response>
        /// <response code="403">If the refresh token is unknown/invalid, does not exist, or do not belong together with the access token.</response>
        /// <response code="400">If the request is malformed and/or failed validation criteria.</response>
        [Route("refresh")]
        [HttpPost]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TokenResponseObject> Refresh(TokenRequestObject requestObject)
        {
            if (string.IsNullOrWhiteSpace(requestObject.AccessToken) || string.IsNullOrWhiteSpace(requestObject.RefreshToken))
            {
                return Problem(detail: "Access or refresh token is empty or missing", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!JWTService.ValidateTokenAndGetPrincipal(requestObject.AccessToken, true, out ClaimsPrincipal? Principal))
            {
                return Unauthorized();
            }

            string Username = Principal!.Identity?.Name ?? string.Empty;

            if (!Users.ValidateAndGetRefreshToken(Username, requestObject.RefreshToken, out Models.UserRefreshToken? RefreshToken))
            {
                return Forbid();
            }

            Debug.Assert(RefreshToken is not null);

            var Roles = Principal.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);
            JwtToken NewTokens = JWTService.GenerateTokens(Username, Roles);

            Users.RevokeUserRefreshToken(Username, RefreshToken.RefreshToken);
            Users.AddUserRefreshToken(Username, new UserRefreshToken(NewTokens.Refresh, NewTokens.Expires));

            Logger.LogInformation($"Refresh succeeded for user {Username} (refresh: {NewTokens.Refresh} | expiry: {NewTokens.Expires})");

            return Ok(new TokenResponseObject
            {
                Expires = NewTokens.Expires,
                AccessToken = NewTokens.Access,
                RefreshToken = NewTokens.Refresh
            });
        }

        /// <summary>
        /// Revokes a set of tokens using an access and refresh token.
        /// </summary>
        /// <response code="204">If the given set of tokens were revoked.</response>
        /// <response code="400">If the request is malformed and/or failed validation criteria.</response>
        /// <response code="401">If the access token is invalid or unknown.</response>
        /// <response code="403">If the refresh token is unknown/invalid, does not exist, or does not belong together with the access token.</response>
        [Route("revoke")]
        [HttpPost]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Revoke(TokenRequestObject requestObject)
        {
            if (string.IsNullOrWhiteSpace(requestObject.AccessToken) || string.IsNullOrWhiteSpace(requestObject.RefreshToken))
            {
                return Problem(detail: "Access or refresh token is empty or missing", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!JWTService.ValidateTokenAndGetPrincipal(requestObject.AccessToken, true, out ClaimsPrincipal? Principal))
            {
                return Unauthorized();
            }

            string Username = Principal.Identity?.Name ?? string.Empty;

            if (Users.RevokeUserRefreshToken(Username, requestObject.RefreshToken))
                return NoContent();

            return Forbid();
        }
    }
}