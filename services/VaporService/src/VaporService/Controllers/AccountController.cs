using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using VaporService.Models;
using VaporService.Storages;

namespace VaporService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IStorage<string, Fighter> _userStorage;

        public AccountController(IStorage<string, Fighter> userStorage)
        {
            _userStorage = userStorage;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                IncorrectCredentials();
                return BadRequest();
            }

            var user = await _userStorage.Get(model.Name);
            if (user != null && user.Password.Equals(model.Password))
            {
                await Authenticate(model.Name);
                return Ok();
            }

            IncorrectCredentials();
            return StatusCode(403);
        }

        private void IncorrectCredentials()
        {
            ModelState.AddModelError("", "login or password missed");
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(Fighter model)
        {
            if (!ModelState.IsValid)
            {
                IncorrectCredentials();
                return BadRequest();
            }

            if (await _userStorage.Get(model.Name) != null) return Conflict();

            await _userStorage.Put(model.Name, model);
            await Authenticate(model.Name);
            return Ok();
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, userName),
                new(ClaimsIdentity.DefaultRoleClaimType, "EmployeeOnly")
            };
            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}