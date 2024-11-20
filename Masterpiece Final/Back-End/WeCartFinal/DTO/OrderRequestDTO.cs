using static Wecartcore.DTO.OrderDetailsDto;

namespace Wecartcore.DTO
{
    public class OrderRequestDTO
    {
        public int? UserId { get; set; }

        public decimal? Amount { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }



        public string? Status { get; set; }



        public List<OrderItemDTO> Items { get; set; }

    }
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
