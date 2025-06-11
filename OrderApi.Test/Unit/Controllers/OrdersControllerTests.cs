using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApi.Test.Unit.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_Returns201Created()
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

            var expectedResponse = new CreateOrderResponse
            {
                OrderId = request.OrderId,
                Message = "Order created successfully"
            };

            _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            Assert.Equal(nameof(OrdersController.CreateOrder), createdResult.ActionName);

            var response = Assert.IsType<CreateOrderResponse>(createdResult.Value);
            Assert.Equal(expectedResponse.OrderId, response.OrderId);
            Assert.Equal(expectedResponse.Message, response.Message);

            _mockOrderService.Verify(x => x.CreateOrderAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest();
            _controller.ModelState.AddModelError("CustomerName", "Customer name is required");

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            _mockOrderService.Verify(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_DuplicateOrder_Returns409Conflict()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var exceptionMessage = $"Order with ID {request.OrderId} already exists";
            _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            _mockOrderService.Verify(x => x.CreateOrderAsync(request), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Conflict creating order")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_InvalidArgument_Returns400BadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>()
            };

            var exceptionMessage = "Order must contain at least one item";
            _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);


            _mockOrderService.Verify(x => x.CreateOrderAsync(request), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Bad request creating order")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_UnexpectedError_Returns500InternalServerError()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            _mockOrderService.Verify(x => x.CreateOrderAsync(request), Times.Once);

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Unexpected error creating order")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

    }
}
