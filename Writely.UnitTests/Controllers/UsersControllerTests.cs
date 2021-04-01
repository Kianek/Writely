using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Writely.Controllers;
using Writely.Exceptions;
using Writely.Models;
using Writely.Services;
using Xunit;

namespace Writely.UnitTests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Register_NewUser_ReturnsCreated()
        {
            // Arrange
            var registration = CompleteRegistration();
            var controller = new UsersController(PrepUserServiceWithoutUser());

            // Act
            var response = await controller.Register(registration);

            // Assert
            response.Should().BeOfType<OkResult>();
        }
        
        [Fact]
        public async Task Register_IncompleteRegistration_ReturnsBadRequest()
        {
            // Arrange
            var registration = IncompleteRegistration();
            var controller = PrepControllerForIncompleteInfo();

            // Act
            var response = await controller.Register(registration);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
        }
        
        [Fact]
        public async Task Register_UserAlreadyRegistered_ReturnsBadRequest()
        {
            // Arrange
            var registration = Helpers.GetRegistration();
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.Register(registration);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
        }
        
        [Fact]
        public async Task ChangeEmail_ChangeSuccessful_ReturnsOk()
        {
            // Arrange
            var emailUpdate = Helpers.GetEmailUpdate();
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.ChangeEmail(emailUpdate);

            // Assert
            response.Should().BeOfType<OkResult>();
        }
        
        [Fact]
        public async Task ChangeEmail_IncompleteAccountUpdate_ReturnsBadRequest()
        {
            // Arrange
            var accountUpdate = Helpers.GetEmailUpdate();
            accountUpdate.EmailUpdate!.Email = null;
            var controller = PrepControllerForIncompleteInfo();

            // Act
            var response = await controller.ChangeEmail(accountUpdate);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangeEmail_NewEmailSameAsOld_ReturnsBadRequest()
        {
            // Arrange
            var accountUpdate = Helpers.GetEmailUpdate("old@email.com");
            var mockUserService = GetMockUserService();
            mockUserService.Setup(us =>
                    us.ChangeEmail(It.IsAny<AccountUpdate>()))
                .ReturnsAsync(() => IdentityResult.Failed());
            var controller = new UsersController(mockUserService.Object);

            // Act
            var response = await controller.ChangeEmail(accountUpdate);

            // Assert
            response.Should().BeOfType<BadRequestResult>();
        }
        
        [Fact]
        public async Task ChangePassword_ChangeSuccessful_ReturnsOk()
        {
            // Arrange
            var accountUpdate = Helpers.GetPasswordUpdate();
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.ChangePassword(accountUpdate);

            // Assert
            response.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ChangePassword_PasswordUpdateNull_ReturnsBadRequest()
        {
            // Arrange
            var accountUpdate = Helpers.GetPasswordUpdate();
            accountUpdate.PasswordUpdate!.ConfirmPassword = "TotallyDifferentPW123!";
            var controller = PrepControllerForIncompleteInfo();

            // Act
            var response = await controller.ChangePassword(accountUpdate);

            // Assert
            response.Should().BeOfType<BadRequestResult>();
        }
        
        [Fact]
        public async Task ChangePassword_IncompleteAccountUpdate_ReturnsBadRequest()
        {
            // Arrange
            var accountUpdate = Helpers.GetPasswordUpdate();
            accountUpdate.PasswordUpdate!.Password = "Goobledyblech";
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.ChangePassword(accountUpdate);

            // Assert
            response.Should().BeOfType<BadRequestResult>();
        }
        
        [Fact]
        public async Task DeleteAccount_AccountDeleted_ReturnsOk()
        {
            // Arrange
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.DeleteAccount("UserId");

            // Assert
            response.Should().BeOfType<OkResult>();
        }
        
        [Fact]
        public async Task DeleteAccount_AccountNotFound_ReturnsBadRequest()
        {
            // Arrange
            var controller = PrepControllerWithUser();

            // Act
            var response = await controller.DeleteAccount("UserIdDelete");

            // Assert
            response.Should().BeOfType<BadRequestResult>();
        }

        private Registration CompleteRegistration()
            => new()
            {
                Username = "fancy.mcfancypants",
                Email = "fancy@gmail.com",
                FirstName = "Fancy",
                LastName = "McFancypants",
                Password = "SuperSecretPassword123!",
                ConfirmPassword = "SuperSecretPassword123!"
            };

        private Registration IncompleteRegistration() => new();

        private IUserService PrepUserServiceWithoutUser()
        {
            var userService = GetMockUserService();
            userService.Setup(us => us.CreateAccount(It.IsAny<Registration>()))
                .ReturnsAsync(() => IdentityResult.Success);

            userService.Setup(us => us.CreateAccount(IncompleteRegistration()))
                .Throws<IncompleteRegistrationException>();
            
            return userService.Object;
        }
        
        private IUserService PrepUserServiceWithUser()
        {
            var userService = GetMockUserService();
            userService.Setup(us => us.CreateAccount(It.IsAny<Registration>()))
                .Throws<DuplicateUserException>();
            userService.Setup(us => us.ChangeEmail(It.IsAny<AccountUpdate>()))
                .ReturnsAsync(() => IdentityResult.Success);
            userService.Setup(us => us.ChangePassword(It.IsAny<AccountUpdate>()))
                .ReturnsAsync(() => IdentityResult.Success);
            userService.Setup(us => us.DeleteAccount("UserId"))
                .ReturnsAsync(() => IdentityResult.Success);
            userService.Setup(us => us.DeleteAccount("UserIdDelete"))
                .ReturnsAsync(() => IdentityResult.Failed());
            
            return userService.Object;
        }

        private IUserService PrepUserServiceForIncompleteInfo()
        {
            var userService = GetMockUserService();
            userService.Setup(us => us.CreateAccount(It.IsAny<Registration>()))
                .Throws<MissingInformationException>();
            userService.Setup(us => us.ChangeEmail(It.IsAny<AccountUpdate>()))
                .Throws<MissingInformationException>();
            userService.Setup(us => us.ChangePassword(It.IsAny<AccountUpdate>()))
                .Throws<MissingInformationException>();
            
            return userService.Object;
        }

        private UsersController PrepControllerForIncompleteInfo()
            => new(PrepUserServiceForIncompleteInfo());

        private UsersController PrepControllerWithoutUser()
            => new (PrepUserServiceWithoutUser());

        private UsersController PrepControllerWithUser()
            => new(PrepUserServiceWithUser());

        private Mock<IUserService> GetMockUserService() => new ();
    }
}