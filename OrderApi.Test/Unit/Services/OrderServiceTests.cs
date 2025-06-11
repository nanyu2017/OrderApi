using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApi.Test.Unit.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            _orderService = new OrderService(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            var order = new Order
            {
                OrderId = request.OrderId,
                CustomerName = request.CustomerName,
                CreatedAt = request.CreatedAt,
                Items = new List<OrderItem>()
            };

            _mockRepository.Setup(x => x.ExistsAsync(request.OrderId))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.OrderId, result.OrderId);
            Assert.Equal("Order created successfully", result.Message);

            _mockRepository.Verify(x => x.ExistsAsync(request.OrderId), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_DuplicateOrderId_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            _mockRepository.Setup(x => x.ExistsAsync(request.OrderId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(request));

            _mockRepository.Verify(x => x.ExistsAsync(request.OrderId), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyItems_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>()
            };

            _mockRepository.Setup(x => x.ExistsAsync(request.OrderId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _orderService.CreateOrderAsync(request));

            _mockRepository.Verify(x => x.ExistsAsync(request.OrderId), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
        }


        [Fact]
        public async Task CreateOrderAsync_ItemsWithDuplicateProductIds_AllowsCreation()
        {
            // Arrange - Business decision: allow duplicate product IDs (user can order same product multiple times)
            var productId = Guid.NewGuid();
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest { ProductId = productId, Quantity = 2 },
                    new OrderItemRequest { ProductId = productId, Quantity = 3 }
                }
            };

            _mockRepository.Setup(x => x.ExistsAsync(request.OrderId)).ReturnsAsync(false);
            _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .ReturnsAsync(new Order { OrderId = request.OrderId });

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.OrderId, result.OrderId);
        }
    }
}
