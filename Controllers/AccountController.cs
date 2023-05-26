using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Exp.Extensions;

namespace Exp.Controllers
{
    [AllowAnonymous, Route("account")]
    public class AccountController : Controller
    {
        [HttpGet("~/signin")]
        public async Task<IActionResult> SignIn()
        { 
            return View("SignIn", await HttpContext.GetExternalProvidersAsync()); 
        }

        [HttpPost("~/signin")]
        public async Task<IActionResult> SignIn([FromForm] string provider)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            if (! await HttpContext.IsProviderSupportedAsync(provider))
            {
                return BadRequest();
            }

            var properties = new AuthenticationProperties { RedirectUri = Url.Action("AuthResponse") };

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            //original : return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);

            return Challenge(properties, provider);
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public IActionResult SignOutCurrentUser()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        //THESE MAY NOT BE NEEDED

        //[Route("google-login")]
        //public IActionResult GoogleLogin()
        //{
        //    var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };

        //    return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        //}

        [Route("auth-response")]
        public async Task<IActionResult> AuthResponse()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result == null)
            {
                return NotFound();
            }

            var claims = result?.Principal?.Identities?.FirstOrDefault()?
                    .Claims;

            var username = claims?.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;

            TempData["Message"] = username;

            return RedirectToRoute(new { controller = "Responses", action = "Index" });

        }
    }


}
