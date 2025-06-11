using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Application.Repositories;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApi.Test.Unit.Repositories
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly OrdersDbContext _context;
        private readonly Mock<ILogger<OrderRepository>> _mockLogger;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new OrdersDbContext(options);
            _mockLogger = new Mock<ILogger<OrderRepository>>();
            _repository = new OrderRepository(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidOrder_SavesSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        Quantity = 2
                    }
                }
            };

            // Act
            var result = await _repository.CreateAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderId, result.OrderId);

            // Verify order was saved to database
            var savedOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            Assert.NotNull(savedOrder);
            Assert.Equal(order.CustomerName, savedOrder.CustomerName);
            Assert.Single(savedOrder.Items);
        }



        [Fact]
        public async Task ExistsAsync_ExistingOrder_ReturnsTrue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsAsync(orderId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingOrder_ReturnsFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var exists = await _repository.ExistsAsync(nonExistingId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateAsync_MultipleItems_SavesAllItems()
        {
            // Arrange
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        Quantity = 3
                    },
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        Quantity = 1
                    }
                }
            };

            // Act
            await _repository.CreateAsync(order);

            // Assert
            var savedOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            Assert.NotNull(savedOrder);
            Assert.Equal(3, savedOrder.Items.Count);
            Assert.All(savedOrder.Items, item => Assert.True(item.Quantity > 0));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}