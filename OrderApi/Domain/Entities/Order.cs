using System.ComponentModel.DataAnnotations;

namespace OrderApi.Domain.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public decimal Total { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
