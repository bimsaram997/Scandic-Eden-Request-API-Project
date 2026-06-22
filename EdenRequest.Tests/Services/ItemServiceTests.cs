using EdenRequest.Api.Data;
using EdenRequest.Api.Repositories;
using EdenRequest.Api.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdenRequest.Tests.Services
{
    public class ItemServiceTests
    {
        [Fact]
        public async Task CreateItemAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<IItemRepository>();

            // Force the mock repo to return null (simulating that category ID 99 does not exist)
            mockRepo.Setup(repo => repo.GetCategoryByIdAsync(99))
                    .ReturnsAsync((ItemCategory?)null);

            var service = new ItemService(mockRepo.Object);

            // Act & Assert 
            // Check ArgumentException!
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateItemAsync("Wine Glass", 99)
            );

            // Optional: You can also verify your custom error string message matches exactly!
            Assert.Equal("Category with ID 99 does not exist.", exception.Message);
        }
    }
}
