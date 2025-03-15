using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserBL _userService;

        public AuthController(IUserBL userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDTO request)
        {
            bool isRegistered = _userService.RegisterUser(request);
            if (!isRegistered)
                return BadRequest("User with this email already exists.");

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserDTO request)
        {
            var token = _userService.LoginUser(request);
            if (token == null)
                return Unauthorized("Invalid email or password.");

            return Ok(new { Token = token });
        }
    }
}
