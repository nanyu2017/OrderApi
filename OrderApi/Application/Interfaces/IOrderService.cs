using OrderApi.Application.DTOs;

namespace OrderApi.Application.Interfaces
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    }
}
