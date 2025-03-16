using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;

namespace AddressBook.Controllers
{
    /// <summary>
    /// API Controller for User Authentication.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserBL _userService;

        public AuthController(IUserBL userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user if the email does not already exist.
        /// </summary>
        /// <param name="request">User registration details (email, password, etc.).</param>
        /// <returns>Success message if registered, otherwise an error message.</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDTO request)
        {
            bool isRegistered = _userService.RegisterUser(request);
            if (!isRegistered)
                return BadRequest(new ResponseBody<string> { Success = false, Message = "User with this email already exists." });

            return Ok(new ResponseBody<string> { Success = true, Message = "User registered successfully." });
        }

        /// <summary>
        /// Authenticates the user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="request">User login credentials (email & password).</param>
        /// <returns>JWT token upon successful login.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserDTO request)
        {
            var token = _userService.LoginUser(request);
            if (token == null)
                return Unauthorized(new ResponseBody<string> { Success = false, Message = "Invalid email or password." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Login successful.", Data = token });
        }

        /// <summary>
        /// Sends a password reset email if the provided email exists.
        /// </summary>
        /// <param name="request">User's email for password reset.</param>
        /// <returns>Success message if email exists, otherwise an error.</returns>
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            bool isSent = _userService.ForgotPassword(request.Email);
            if (!isSent)
                return BadRequest(new ResponseBody<string> { Success = false, Message = "Email not found." });

            return Ok(new ResponseBody<string> { Success = true, Message = "Reset password email sent successfully." });
        }

        /// <summary>
        /// Serves an HTML password reset form when accessed with a valid token.
        /// </summary>
        /// <param name="token">JWT token for password reset.</param>
        /// <returns>HTML form for password reset.</returns>
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

        /// <summary>
        /// Resets the user's password using the provided token and new password.
        /// </summary>
        /// <param name="request">Token and new password details.</param>
        /// <returns>Success message if reset is successful, otherwise an error.</returns>
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

        /// <summary>
        /// Handles password reset requests submitted via the HTML form.
        /// </summary>
        /// <param name="token">JWT token for password reset.</param>
        /// <param name="newPassword">New password to set.</param>
        /// <returns>Success message if reset is successful, otherwise an error.</returns>
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
