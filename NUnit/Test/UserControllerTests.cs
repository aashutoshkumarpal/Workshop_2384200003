using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.Test
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserBL> _userServiceMock;
        private AuthController _authController;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserBL>();
            _authController = new AuthController(_userServiceMock.Object);
        }

        [Test]
        public void Register_ValidUser_ReturnsSuccess()
        {
            var request = new RegisterUserDTO { Email = "test@example.com", Password = "Test@123" };
            _userServiceMock.Setup(u => u.RegisterUser(request)).Returns(true);

            var result = _authController.Register(request) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void Login_ValidCredentials_ReturnsToken()
        {
            var request = new LoginUserDTO { Email = "test@example.com", Password = "Test@123" };
            _userServiceMock.Setup(u => u.LoginUser(request)).Returns("ValidToken");

            var result = _authController.Login(request) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ForgotPassword_ValidEmail_ReturnsSuccess()
        {
            _userServiceMock.Setup(u => u.ForgotPassword("test@example.com")).Returns(true);

            var result = _authController.ForgotPassword(new ForgotPasswordDTO { Email = "test@example.com" }) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ResetPassword_ValidToken_ReturnsSuccess()
        {
            _userServiceMock.Setup(u => u.ResetPassword("ValidToken", "NewPass@123")).Returns(true);

            var result = _authController.ResetPassword(new ResetPasswordDTO { Token = "ValidToken", NewPassword = "NewPass@123" }) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
