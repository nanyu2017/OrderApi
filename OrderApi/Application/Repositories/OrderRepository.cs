using Microsoft.EntityFrameworkCore;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Data;

namespace OrderApi.Application.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrdersDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order with ID: {OrderId}", order.OrderId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid orderId)
        {
            try
            {
                return await _context.Orders
                    .AnyAsync(o => o.OrderId == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order exists with ID: {OrderId}", orderId);
                throw;
            }
        }
    }
}
