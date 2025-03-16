using BusinessLayer.Interface;
using ModelLayer.Model;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Testing.TestClasses
{
    public class UserTests
    {
        private Mock<IUserBL> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserBL>();
        }

        [Test]
        public void RegisterUser_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var userDto = new RegisterUserDTO
            {
                FullName = "John",
                Email = "john@example.com",
                Password = "SecurePass123!"
            };

            _userServiceMock.Setup(service => service.RegisterUser(userDto))
                            .Returns(true);

            // Act
            var result = _userServiceMock.Object.RegisterUser(userDto);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void LoginUser_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginDto = new LoginUserDTO
            {
                Email = "john@example.com",
                Password = "SecurePass123!"
            };

            _userServiceMock.Setup(service => service.LoginUser(loginDto))
                            .Returns("jwt_token_here");

            // Act
            var token = _userServiceMock.Object.LoginUser(loginDto);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);
        }
    }
}
