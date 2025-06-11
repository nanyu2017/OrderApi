using System.ComponentModel.DataAnnotations;

namespace OrderApi.Application.DTOs
{
    public class CreateOrderRequest
    {
        public Guid OrderId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemRequest> Items { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }

    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
