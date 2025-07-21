using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.UserManagement.Controllers;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.UserManagement.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace YopoAPI.Tests.Controllers
{
    public class InvitationsControllerTests
    {
        private readonly Mock<IInvitationService> _invitationServiceMock;
        private readonly InvitationsController _invitationsController;

        public InvitationsControllerTests()
        {
            _invitationServiceMock = new Mock<IInvitationService>();
            _invitationsController = new InvitationsController(_invitationServiceMock.Object);
            _invitationsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim("userId", "1")
                    }, "mock"))
                }
            };
        }

        [Fact]
        public async Task CreateInvitation_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createInvitationDto = new CreateInvitationDto
            {
                Email = "invitee@example.com",
                RoleId = 1,
                ExpiryDays = 7
            };

            var invitationDto = new InvitationDto
            {
                Id = 1,
                Email = createInvitationDto.Email,
                RoleName = "Admin",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _invitationServiceMock.Setup(x => x.CreateInvitationAsync(createInvitationDto, 1))
                .ReturnsAsync(invitationDto);

            // Act
            var result = await _invitationsController.CreateInvitation(createInvitationDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedInvitation = Assert.IsType<InvitationDto>(createdAtActionResult.Value);
            Assert.Equal(createInvitationDto.Email, returnedInvitation.Email);
        }

        [Fact]
        public async Task GetInvitations_ReturnsOkResultWithInvitationsList()
        {
            // Arrange
            var invitations = new List<InvitationDto>
            {
                new InvitationDto { Id = 1, Email = "invitee1@example.com", RoleName = "Admin" },
                new InvitationDto { Id = 2, Email = "invitee2@example.com", RoleName = "User" }
            };

            _invitationServiceMock.Setup(x => x.GetAllInvitationsAsync())
                .ReturnsAsync(invitations);

            // Act
            var result = await _invitationsController.GetInvitations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInvitations = Assert.IsAssignableFrom<IEnumerable<InvitationDto>>(okResult.Value);
            Assert.Equal(2, returnedInvitations.Count());
        }

        [Fact]
        public async Task GetInvitation_WithValidId_ReturnsOkResultWithInvitation()
        {
            // Arrange
            var invitationId = 1;
            var invitation = new InvitationDto
            {
                Id = invitationId,
                Email = "invitee@example.com",
                RoleName = "Admin",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _invitationServiceMock.Setup(x => x.GetInvitationByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            // Act
            var result = await _invitationsController.GetInvitation(invitationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInvitation = Assert.IsType<InvitationDto>(okResult.Value);
            Assert.Equal(invitationId, returnedInvitation.Id);
        }

        [Fact]
        public async Task GetInvitation_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invitationId = 999;
            _invitationServiceMock.Setup(x => x.GetInvitationByIdAsync(invitationId))
                .ReturnsAsync((InvitationDto?)null);

            // Act
            var result = await _invitationsController.GetInvitation(invitationId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CheckInvitation_WithValidEmail_ReturnsOkWithInvitationCheck()
        {
            // Arrange
            var email = "invitee@example.com";
            var invitationCheckDto = new InvitationCheckDto { IsInvited = true, RoleName = "Admin" };

            _invitationServiceMock.Setup(x => x.CheckInvitationAsync(email))
                .ReturnsAsync(invitationCheckDto);

            // Act
            var result = await _invitationsController.CheckInvitation(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCheck = Assert.IsType<InvitationCheckDto>(okResult.Value);
            Assert.True(returnedCheck.IsInvited);
        }

        [Fact]
        public async Task DeleteInvitation_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var invitationId = 1;
            _invitationServiceMock.Setup(x => x.DeleteInvitationAsync(invitationId))
                .ReturnsAsync(true);

            // Act
            var result = await _invitationsController.DeleteInvitation(invitationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteInvitation_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invitationId = 999;
            _invitationServiceMock.Setup(x => x.DeleteInvitationAsync(invitationId))
                .ReturnsAsync(false);

            // Act
            var result = await _invitationsController.DeleteInvitation(invitationId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
