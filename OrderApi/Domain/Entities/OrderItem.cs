using System.ComponentModel.DataAnnotations;

namespace OrderApi.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        public Order Order { get; set; } = null!;
    }
}
