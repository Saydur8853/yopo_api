using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.PolicyManagement.Controllers;
using YopoAPI.Modules.PolicyManagement.DTOs;
using YopoAPI.Modules.PolicyManagement.Services;

namespace YopoAPI.Tests.Controllers
{
    public class PolicyControllerTests
    {
        private readonly Mock<IPolicyService> _policyServiceMock;
        private readonly PolicyController _policyController;

        public PolicyControllerTests()
        {
            _policyServiceMock = new Mock<IPolicyService>();
            _policyController = new PolicyController(_policyServiceMock.Object);
        }

        [Fact]
        public async Task GetTerms_ReturnsOkResultWithTermsPolicy()
        {
            // Arrange
            var policy = new PolicyDto { Id = 1, Type = "terms", Content = "Terms and Conditions" };

            _policyServiceMock.Setup(x => x.GetPolicyByTypeAsync("terms"))
                .ReturnsAsync(policy);

            // Act
            var result = await _policyController.GetTerms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
            Assert.Equal("terms", returnedPolicy.Type);
        }

        [Fact]
        public async Task GetTerms_ReturnsNotFound_WhenTermsAreMissing()
        {
            // Arrange
            _policyServiceMock.Setup(x => x.GetPolicyByTypeAsync("terms"))
                .ReturnsAsync((PolicyDto?)null);

            // Act
            var result = await _policyController.GetTerms();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPrivacyPolicy_ReturnsOkResultWithPrivacyPolicy()
        {
            // Arrange
            var policy = new PolicyDto { Id = 2, Type = "privacy", Content = "Privacy Policy" };

            _policyServiceMock.Setup(x => x.GetPolicyByTypeAsync("privacy"))
                .ReturnsAsync(policy);

            // Act
            var result = await _policyController.GetPrivacyPolicy();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
            Assert.Equal("privacy", returnedPolicy.Type);
        }

        [Fact]
        public async Task GetPrivacyPolicy_ReturnsNotFound_WhenPrivacyPolicyIsMissing()
        {
            // Arrange
            _policyServiceMock.Setup(x => x.GetPolicyByTypeAsync("privacy"))
                .ReturnsAsync((PolicyDto?)null);

            // Act
            var result = await _policyController.GetPrivacyPolicy();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllPolicies_ReturnsOkResultWithPoliciesList()
        {
            // Arrange
            var policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Type = "terms", Content = "Terms and Conditions" },
                new PolicyDto { Id = 2, Type = "privacy", Content = "Privacy Policy" }
            };

            _policyServiceMock.Setup(x => x.GetAllPoliciesAsync())
                .ReturnsAsync(policies);

            // Act
            var result = await _policyController.GetAllPolicies();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPolicies = Assert.IsAssignableFrom<IEnumerable<PolicyDto>>(okResult.Value);
            Assert.Equal(2, returnedPolicies.Count());
        }

        [Fact]
        public async Task CreatePolicy_WithValidData_ReturnsOkResultWithCreatedPolicy()
        {
            // Arrange
            var createPolicyDto = new CreatePolicyDto { Type = "cookies", Content = "Cookies Policy" };
            var createdPolicy = new PolicyDto { Id = 3, Type = createPolicyDto.Type, Content = createPolicyDto.Content };

            _policyServiceMock.Setup(x => x.CreatePolicyAsync(createPolicyDto))
                .ReturnsAsync(createdPolicy);

            // Act
            var result = await _policyController.CreatePolicy(createPolicyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
            Assert.Equal(createPolicyDto.Type, returnedPolicy.Type);
        }

        [Fact]
        public async Task UpdatePolicy_WithValidData_ReturnsOkResultWithUpdatedPolicy()
        {
            // Arrange
            var policyId = 1;
            var updatePolicyDto = new UpdatePolicyDto { Type = "terms", Content = "Updated Terms and Conditions" };
            var updatedPolicy = new PolicyDto { Id = policyId, Type = updatePolicyDto.Type, Content = updatePolicyDto.Content };

            _policyServiceMock.Setup(x => x.UpdatePolicyAsync(policyId, updatePolicyDto))
                .ReturnsAsync(updatedPolicy);

            // Act
            var result = await _policyController.UpdatePolicy(policyId, updatePolicyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
            Assert.Equal(updatePolicyDto.Content, returnedPolicy.Content);
        }

        [Fact]
        public async Task UpdatePolicy_WithNonExistentPolicy_ReturnsNotFound()
        {
            // Arrange
            var policyId = 999;
            var updatePolicyDto = new UpdatePolicyDto { Type = "terms", Content = "Updated Terms and Conditions" };

            _policyServiceMock.Setup(x => x.UpdatePolicyAsync(policyId, updatePolicyDto))
                .ReturnsAsync((PolicyDto?)null);

            // Act
            var result = await _policyController.UpdatePolicy(policyId, updatePolicyDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeletePolicy_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var policyId = 1;
            _policyServiceMock.Setup(x => x.DeletePolicyAsync(policyId))
                .ReturnsAsync(true);

            // Act
            var result = await _policyController.DeletePolicy(policyId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePolicy_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var policyId = 999;
            _policyServiceMock.Setup(x => x.DeletePolicyAsync(policyId))
                .ReturnsAsync(false);

            // Act
            var result = await _policyController.DeletePolicy(policyId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
