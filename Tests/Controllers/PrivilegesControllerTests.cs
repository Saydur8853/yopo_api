using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.RoleManagement.Controllers;
using YopoAPI.Modules.RoleManagement.DTOs;
using YopoAPI.Modules.RoleManagement.Services;

namespace YopoAPI.Tests.Controllers
{
    public class PrivilegesControllerTests
    {
        private readonly Mock<IPrivilegeService> _privilegeServiceMock;
        private readonly PrivilegesController _privilegesController;

        public PrivilegesControllerTests()
        {
            _privilegeServiceMock = new Mock<IPrivilegeService>();
            _privilegesController = new PrivilegesController(_privilegeServiceMock.Object);
        }

        [Fact]
        public async Task GetPrivileges_ReturnsOkResultWithPrivilegesList()
        {
            // Arrange
            var privileges = new List<PrivilegeDto>
            {
                new PrivilegeDto { Id = 1, Name = "Read", Description = "Read Permission" },
                new PrivilegeDto { Id = 2, Name = "Write", Description = "Write Permission" }
            };

            _privilegeServiceMock.Setup(x => x.GetAllPrivilegesAsync())
                .ReturnsAsync(privileges);

            // Act
            var result = await _privilegesController.GetPrivileges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrivileges = Assert.IsAssignableFrom<IEnumerable<PrivilegeDto>>(okResult.Value);
            Assert.Equal(2, returnedPrivileges.Count());
        }

        [Fact]
        public async Task GetPrivilege_WithValidId_ReturnsOkResultWithPrivilege()
        {
            // Arrange
            var privilegeId = 1;
            var privilege = new PrivilegeDto
            {
                Id = privilegeId,
                Name = "Read",
                Description = "Read Permission"
            };

            _privilegeServiceMock.Setup(x => x.GetPrivilegeByIdAsync(privilegeId))
                .ReturnsAsync(privilege);

            // Act
            var result = await _privilegesController.GetPrivilege(privilegeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrivilege = Assert.IsType<PrivilegeDto>(okResult.Value);
            Assert.Equal(privilegeId, returnedPrivilege.Id);
            Assert.Equal("Read", returnedPrivilege.Name);
        }

        [Fact]
        public async Task GetPrivilege_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var privilegeId = 999;
            _privilegeServiceMock.Setup(x => x.GetPrivilegeByIdAsync(privilegeId))
                .ReturnsAsync((PrivilegeDto?)null);

            // Act
            var result = await _privilegesController.GetPrivilege(privilegeId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreatePrivilege_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createPrivilegeDto = new CreatePrivilegeDto
            {
                Name = "Execute",
                Description = "Execute Permission"
            };

            var createdPrivilege = new PrivilegeDto
            {
                Id = 1,
                Name = createPrivilegeDto.Name,
                Description = createPrivilegeDto.Description
            };

            _privilegeServiceMock.Setup(x => x.CreatePrivilegeAsync(createPrivilegeDto))
                .ReturnsAsync(createdPrivilege);

            // Act
            var result = await _privilegesController.CreatePrivilege(createPrivilegeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedPrivilege = Assert.IsType<PrivilegeDto>(createdAtActionResult.Value);
            Assert.Equal(createPrivilegeDto.Name, returnedPrivilege.Name);
        }

        [Fact]
        public async Task UpdatePrivilege_WithValidData_ReturnsOkResultWithUpdatedPrivilege()
        {
            // Arrange
            var privilegeId = 1;
            var updatePrivilegeDto = new UpdatePrivilegeDto
            {
                Name = "UpdatedRead",
                Description = "Updated Read Permission"
            };

            var updatedPrivilege = new PrivilegeDto
            {
                Id = privilegeId,
                Name = updatePrivilegeDto.Name,
                Description = updatePrivilegeDto.Description
            };

            _privilegeServiceMock.Setup(x => x.UpdatePrivilegeAsync(privilegeId, updatePrivilegeDto))
                .ReturnsAsync(updatedPrivilege);

            // Act
            var result = await _privilegesController.UpdatePrivilege(privilegeId, updatePrivilegeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrivilege = Assert.IsType<PrivilegeDto>(okResult.Value);
            Assert.Equal(updatePrivilegeDto.Name, returnedPrivilege.Name);
        }

        [Fact]
        public async Task UpdatePrivilege_WithNonExistentPrivilege_ReturnsNotFound()
        {
            // Arrange
            var privilegeId = 999;
            var updatePrivilegeDto = new UpdatePrivilegeDto
            {
                Name = "UpdatedRead",
                Description = "Updated Read Permission"
            };

            _privilegeServiceMock.Setup(x => x.UpdatePrivilegeAsync(privilegeId, updatePrivilegeDto))
                .ReturnsAsync((PrivilegeDto?)null);

            // Act
            var result = await _privilegesController.UpdatePrivilege(privilegeId, updatePrivilegeDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeletePrivilege_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var privilegeId = 1;
            _privilegeServiceMock.Setup(x => x.DeletePrivilegeAsync(privilegeId))
                .ReturnsAsync(true);

            // Act
            var result = await _privilegesController.DeletePrivilege(privilegeId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePrivilege_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var privilegeId = 999;
            _privilegeServiceMock.Setup(x => x.DeletePrivilegeAsync(privilegeId))
                .ReturnsAsync(false);

            // Act
            var result = await _privilegesController.DeletePrivilege(privilegeId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
