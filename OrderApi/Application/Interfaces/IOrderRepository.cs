using OrderApi.Domain.Entities;

namespace OrderApi.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);

        Task<bool> ExistsAsync(Guid orderId);
    }
}
