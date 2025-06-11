using AutoMapper;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;

namespace OrderApi.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Creating order with ID: {OrderId}", request.OrderId);

                // Check if order already exists
                if (await _orderRepository.ExistsAsync(request.OrderId))
                {
                    _logger.LogWarning("Order with ID {OrderId} already exists", request.OrderId);
                    throw new InvalidOperationException($"Order with ID {request.OrderId} already exists");
                }

                if (!request.Items.Any())
                {
                    throw new ArgumentException("Order must contain at least one item");
                }

                var order = new Order
                {
                    OrderId = request.OrderId,
                    CustomerName = request.CustomerName,
                    CreatedAt = request.CreatedAt,
                    Items = request.Items.Select(item => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = request.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
                };

                var createdOrder = await _orderRepository.CreateAsync(order);

                _logger.LogInformation("Successfully created order with ID: {OrderId}", createdOrder.OrderId);

                return new CreateOrderResponse
                {
                    OrderId = createdOrder.OrderId,
                    Message = "Order created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order with ID: {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}
