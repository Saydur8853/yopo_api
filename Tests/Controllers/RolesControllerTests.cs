using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.RoleManagement.Controllers;
using YopoAPI.Modules.RoleManagement.DTOs;
using YopoAPI.Modules.RoleManagement.Services;

namespace YopoAPI.Tests.Controllers
{
    public class RolesControllerTests
    {
        private readonly Mock<IRoleService> _roleServiceMock;
        private readonly Mock<IPrivilegeService> _privilegeServiceMock;
        private readonly RolesController _rolesController;

        public RolesControllerTests()
        {
            _roleServiceMock = new Mock<IRoleService>();
            _privilegeServiceMock = new Mock<IPrivilegeService>();
            _rolesController = new RolesController(_roleServiceMock.Object, _privilegeServiceMock.Object);
        }

        [Fact]
        public async Task GetRoles_ReturnsOkResultWithRolesList()
        {
            // Arrange
            var roles = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin", Description = "Administrator Role" },
                new RoleDto { Id = 2, Name = "User", Description = "Standard User Role" }
            };

            _roleServiceMock.Setup(x => x.GetAllRolesAsync())
                .ReturnsAsync(roles);

            // Act
            var result = await _rolesController.GetRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value);
            Assert.Equal(2, returnedRoles.Count());
        }

        [Fact]
        public async Task GetRole_WithValidId_ReturnsOkResultWithRole()
        {
            // Arrange
            var roleId = 1;
            var role = new RoleDto 
            { 
                Id = roleId, 
                Name = "Admin", 
                Description = "Administrator Role" 
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(role);

            // Act
            var result = await _rolesController.GetRole(roleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRole = Assert.IsType<RoleDto>(okResult.Value);
            Assert.Equal(roleId, returnedRole.Id);
            Assert.Equal("Admin", returnedRole.Name);
        }

        [Fact]
        public async Task GetRole_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var roleId = 999;
            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync((RoleDto?)null);

            // Act
            var result = await _rolesController.GetRole(roleId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateRole_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto
            {
                Name = "NewRole",
                Description = "New Role Description"
            };

            var createdRole = new RoleDto
            {
                Id = 1,
                Name = createRoleDto.Name,
                Description = createRoleDto.Description
            };

            _roleServiceMock.Setup(x => x.RoleExistsAsync(createRoleDto.Name))
                .ReturnsAsync(false);

            _roleServiceMock.Setup(x => x.CreateRoleAsync(createRoleDto))
                .ReturnsAsync(createdRole);

            // Act
            var result = await _rolesController.CreateRole(createRoleDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedRole = Assert.IsType<RoleDto>(createdAtActionResult.Value);
            Assert.Equal(createRoleDto.Name, returnedRole.Name);
        }

        [Fact]
        public async Task CreateRole_WithExistingName_ReturnsBadRequest()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto
            {
                Name = "ExistingRole",
                Description = "Role Description"
            };

            _roleServiceMock.Setup(x => x.RoleExistsAsync(createRoleDto.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _rolesController.CreateRole(createRoleDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateRole_WithValidData_ReturnsOkResultWithUpdatedRole()
        {
            // Arrange
            var roleId = 1;
            var updateRoleDto = new UpdateRoleDto
            {
                Name = "UpdatedRole",
                Description = "Updated Role Description"
            };

            var existingRole = new RoleDto
            {
                Id = roleId,
                Name = "OldRole",
                Description = "Old Role Description"
            };

            var updatedRole = new RoleDto
            {
                Id = roleId,
                Name = updateRoleDto.Name,
                Description = updateRoleDto.Description
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(existingRole);

            _roleServiceMock.Setup(x => x.UpdateRoleAsync(roleId, updateRoleDto))
                .ReturnsAsync(updatedRole);

            // Act
            var result = await _rolesController.UpdateRole(roleId, updateRoleDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRole = Assert.IsType<RoleDto>(okResult.Value);
            Assert.Equal(updateRoleDto.Name, returnedRole.Name);
        }

        [Fact]
        public async Task UpdateRole_WithNonExistentRole_ReturnsNotFound()
        {
            // Arrange
            var roleId = 999;
            var updateRoleDto = new UpdateRoleDto
            {
                Name = "UpdatedRole",
                Description = "Updated Role Description"
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync((RoleDto?)null);

            // Act
            var result = await _rolesController.UpdateRole(roleId, updateRoleDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteRole_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var roleId = 1;
            _roleServiceMock.Setup(x => x.RoleHasUsersAsync(roleId))
                .ReturnsAsync(false);

            _roleServiceMock.Setup(x => x.DeleteRoleAsync(roleId))
                .ReturnsAsync(true);

            // Act
            var result = await _rolesController.DeleteRole(roleId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRole_WithRoleHavingUsers_ReturnsBadRequest()
        {
            // Arrange
            var roleId = 1;
            _roleServiceMock.Setup(x => x.RoleHasUsersAsync(roleId))
                .ReturnsAsync(true);

            // Act
            var result = await _rolesController.DeleteRole(roleId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AssignPrivilegesToRole_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var roleId = 1;
            var assignPrivilegesDto = new AssignPrivilegesToRoleDto
            {
                PrivilegeIds = new List<int> { 1, 2, 3 }
            };

            var existingRole = new RoleDto
            {
                Id = roleId,
                Name = "TestRole",
                Description = "Test Role Description"
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(existingRole);

            _privilegeServiceMock.Setup(x => x.AssignPrivilegesToRoleAsync(roleId, assignPrivilegesDto.PrivilegeIds))
                .ReturnsAsync(true);

            // Act
            var result = await _rolesController.AssignPrivilegesToRole(roleId, assignPrivilegesDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task AssignPrivilegesToRole_WithNonExistentRole_ReturnsNotFound()
        {
            // Arrange
            var roleId = 999;
            var assignPrivilegesDto = new AssignPrivilegesToRoleDto
            {
                PrivilegeIds = new List<int> { 1, 2, 3 }
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync((RoleDto?)null);

            // Act
            var result = await _rolesController.AssignPrivilegesToRole(roleId, assignPrivilegesDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetRolePrivileges_WithValidRoleId_ReturnsOkResultWithPrivileges()
        {
            // Arrange
            var roleId = 1;
            var existingRole = new RoleDto
            {
                Id = roleId,
                Name = "TestRole",
                Description = "Test Role Description"
            };

            var privileges = new List<PrivilegeDto>
            {
                new PrivilegeDto { Id = 1, Name = "Read", Description = "Read Permission" },
                new PrivilegeDto { Id = 2, Name = "Write", Description = "Write Permission" }
            };

            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(existingRole);

            _privilegeServiceMock.Setup(x => x.GetRolePrivilegesAsync(roleId))
                .ReturnsAsync(privileges);

            // Act
            var result = await _rolesController.GetRolePrivileges(roleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrivileges = Assert.IsAssignableFrom<IEnumerable<PrivilegeDto>>(okResult.Value);
            Assert.Equal(2, returnedPrivileges.Count());
        }

        [Fact]
        public async Task GetRolePrivileges_WithNonExistentRole_ReturnsNotFound()
        {
            // Arrange
            var roleId = 999;
            _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync((RoleDto?)null);

            // Act
            var result = await _rolesController.GetRolePrivileges(roleId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task SetRoleHierarchy_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var roleHierarchyDto = new RoleHierarchyDto
            {
                RoleId = 1,
                ParentRoleId = 2,
                HierarchyLevel = 1
            };

            _privilegeServiceMock.Setup(x => x.SetRoleHierarchyAsync(
                roleHierarchyDto.RoleId,
                roleHierarchyDto.ParentRoleId,
                roleHierarchyDto.HierarchyLevel))
                .ReturnsAsync(true);

            // Act
            var result = await _rolesController.SetRoleHierarchy(roleHierarchyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetRoleHierarchy_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var roleHierarchyDto = new RoleHierarchyDto
            {
                RoleId = 1,
                ParentRoleId = 2,
                HierarchyLevel = 1
            };

            _privilegeServiceMock.Setup(x => x.SetRoleHierarchyAsync(
                roleHierarchyDto.RoleId,
                roleHierarchyDto.ParentRoleId,
                roleHierarchyDto.HierarchyLevel))
                .ReturnsAsync(false);

            // Act
            var result = await _rolesController.SetRoleHierarchy(roleHierarchyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
