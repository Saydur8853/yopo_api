using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.UserManagement.Controllers;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.RoleManagement.DTOs;
using YopoAPI.Modules.Authentication.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace YopoAPI.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _usersController;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _usersController = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResultWithUsersList()
        {
            // Arrange
            var users = new List<UserListDto>
            {
                new UserListDto { Id = 1, Email = "user1@example.com", FirstName = "User", LastName = "One" },
                new UserListDto { Id = 2, Email = "user2@example.com", FirstName = "User", LastName = "Two" }
            };

            _userServiceMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _usersController.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserListDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsOkResultWithUser()
        {
            // Arrange
            var userId = 1;
            var user = new UserDto 
            { 
                Id = userId, 
                Email = "user@example.com", 
                FirstName = "Test", 
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _usersController.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal("user@example.com", returnedUser.Email);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _usersController.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User",
                RoleId = 1,
                Password = "password123"
            };

            var createdUser = new UserDto
            {
                Id = 1,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.UserExistsAsync(createUserDto.Email))
                .ReturnsAsync(false);

            _userServiceMock.Setup(x => x.CreateUserAsync(createUserDto))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _usersController.CreateUser(createUserDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(createdAtActionResult.Value);
            Assert.Equal(createUserDto.Email, returnedUser.Email);
        }

        [Fact]
        public async Task CreateUser_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "existing@example.com",
                FirstName = "New",
                LastName = "User",
                RoleId = 1,
                Password = "password123"
            };

            _userServiceMock.Setup(x => x.UserExistsAsync(createUserDto.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _usersController.CreateUser(createUserDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsOkResultWithUpdatedUser()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = new UpdateUserDto
            {
                Email = "updated@example.com",
                FirstName = "Updated",
                LastName = "User",
                RoleId = 1
            };

            var existingUser = new UserDto
            {
                Id = userId,
                Email = "old@example.com",
                FirstName = "Old",
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            var updatedUser = new UserDto
            {
                Id = userId,
                Email = updateUserDto.Email,
                FirstName = updateUserDto.FirstName,
                LastName = updateUserDto.LastName,
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userServiceMock.Setup(x => x.UserExistsAsync(updateUserDto.Email))
                .ReturnsAsync(false);

            _userServiceMock.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _usersController.UpdateUser(userId, updateUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(updateUserDto.Email, returnedUser.Email);
        }

        [Fact]
        public async Task UpdateUser_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var updateUserDto = new UpdateUserDto
            {
                Email = "updated@example.com",
                FirstName = "Updated",
                LastName = "User",
                RoleId = 1
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _usersController.UpdateUser(userId, updateUserDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            var existingUser = new UserDto
            {
                Id = userId,
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userServiceMock.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _usersController.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _usersController.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetUserByEmail_WithValidEmail_ReturnsOkResultWithUser()
        {
            // Arrange
            var email = "user@example.com";
            var user = new UserDto
            {
                Id = 1,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _usersController.GetUserByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(email, returnedUser.Email);
        }

        [Fact]
        public async Task GetUserByEmail_WithInvalidEmail_ReturnsNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _userServiceMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _usersController.GetUserByEmail(email);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task ToggleUserStatus_WithValidId_ReturnsOkWithMessage()
        {
            // Arrange
            var userId = 1;
            var existingUser = new UserDto
            {
                Id = userId,
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userServiceMock.Setup(x => x.UpdateUserAsync(userId, It.IsAny<UpdateUserDto>()))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _usersController.ToggleUserStatus(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task AssignRole_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var userId = 1;
            var assignRoleDto = new AssignRoleDto { RoleId = 2 };
            var existingUser = new UserDto
            {
                Id = userId,
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userServiceMock.Setup(x => x.AssignRoleToUserAsync(userId, assignRoleDto.RoleId))
                .ReturnsAsync(true);

            // Act
            var result = await _usersController.AssignRole(userId, assignRoleDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task AssignRole_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var assignRoleDto = new AssignRoleDto { RoleId = 2 };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _usersController.AssignRole(userId, assignRoleDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
