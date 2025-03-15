using AutoMapper;
using BusinessLayer.Interface;
using Middleware.Authenticator;
using Middleware.Salting;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRepository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;

        public UserBL(IUserRL userRepository, JwtTokenService jwtTokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
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
    }
}