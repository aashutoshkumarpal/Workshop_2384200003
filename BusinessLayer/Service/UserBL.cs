using AutoMapper;
using BusinessLayer.Interface;
using Middleware.Authenticator;
using Middleware.Email;
using Middleware.Salting;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRepository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;

        public UserBL(IUserRL userRepository, JwtTokenService jwtTokenService, IMapper mapper, EmailService emailService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
            _emailService = emailService;
        }

        public bool RegisterUser(RegisterUserDTO request)
        {
            try
            {
                // Check if the user already exists
                var existingUser = _userRepository.GetUserByEmail(request.Email);
                if (existingUser != null)
                    return false; // User already exists

                // Map DTO to Entity
                var user = _mapper.Map<User>(request);
                user.PasswordHash = PasswordHelper.HashPassword(request.Password);

                _userRepository.AddUser(user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterUser] Error: {ex.Message}");
                return false;
            }
        }

        public string? LoginUser(LoginUserDTO request)
        {
            try
            {
                var user = _userRepository.GetUserByEmail(request.Email);
                if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                    return null;
                var userModel = _mapper.Map<UserModel>(user);

                return _jwtTokenService.GenerateToken(userModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginUser] Error: {ex.Message}");
                return null;
            }
        }

        public bool ForgotPassword(string email)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null) return false; // User not found

            // Generate Reset Token
            string resetToken = _jwtTokenService.GenerateResetToken(user.Email);

            // Encode the token to prevent URL issues
            string encodedToken = HttpUtility.UrlEncode(resetToken);

            // Construct Reset Link
            string resetLink = $"https://localhost:7265/api/Auth/reset-password?token={resetToken}";
            string subject = "Reset Your Password";
            //string body = $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>";
            string body = $"Click the link to reset your password: {resetLink}";


            _emailService.SendEmail(user.Email, subject, body);
            return true;
        }

        public bool ResetPassword(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false; // Invalid token

                // Decode the token before validation
                string decodedToken = HttpUtility.UrlDecode(token);

                // Validate JWT token
                var tokenData = _jwtTokenService.ValidateResetToken(decodedToken);
                if (tokenData == null || !tokenData.ContainsKey(ClaimTypes.Email)) return false;

                string email = tokenData[ClaimTypes.Email].ToString();
                var user = _userRepository.GetUserByEmail(email);
                if (user == null) return false;

                // Hash new password and update user
                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _userRepository.UpdateUser(user);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ResetPassword] Error: {ex.Message}");
                return false;
            }
        } 
    }
}