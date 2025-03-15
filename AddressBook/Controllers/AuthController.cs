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
                return BadRequest(new ResponseBody<string> { Success = false, Message = "User with this email already exists." });

            return Ok(new ResponseBody<string> { Success = true, Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserDTO request)
        {
            var token = _userService.LoginUser(request);
            if (token == null)
                return Unauthorized(new ResponseBody<string> { Success = false, Message = "Invalid email or password." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Login successful.", Data = token });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            bool isSent = _userService.ForgotPassword(request.Email);
            if (!isSent)
                return BadRequest(new ResponseBody<string> { Success = false, Message = "Email not found." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Reset password email sent successfully." });
        }

        [HttpGet("reset-password")]
        public ContentResult ResetPasswordForm([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return new ContentResult { Content = "Token is required.", ContentType = "text/plain" };

            string htmlForm = $@"
                <html>
                <body>
                     <form action='/api/Auth/reset-password-form' method='post'>
                        <input type='hidden' name='token' value='{token}' />
                        <label>New Password:</label>
                        <input type='password' name='newPassword' required />
                        <button type='submit'>Reset Password</button>
                    </form>
                </body>
                 </html>";

            return new ContentResult { Content = htmlForm, ContentType = "text/html" };
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new ResponseBody<string> { Success = false, Message = "Token and new password are required." });

            bool isResetSuccessful = _userService.ResetPassword(request.Token, request.NewPassword);
            if (!isResetSuccessful)
                return Unauthorized(new ResponseBody<string> { Success = false, Message = "Invalid or expired token." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Password reset successfully." });
        }

        [HttpPost("reset-password-form")]
        public IActionResult ResetPasswordForm([FromForm] string token, [FromForm] string newPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
                return BadRequest(new ResponseBody<string> { Success = false, Message = "Token and new password are required." });

            bool isResetSuccessful = _userService.ResetPassword(token, newPassword);
            if (!isResetSuccessful)
                return Unauthorized(new ResponseBody<string> { Success = false, Message = "Invalid or expired token." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Password reset successfully." });
        }
    }
}
