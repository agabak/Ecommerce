namespace Ecommerce.Common.Models
{
    public class Order
    {
        public User? User { get; set; } 

        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "In Process";
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public PaymentType PaymentType { get; set; } = PaymentType.CashOnDelivery;  
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public Guid ProductId { get; set; }  // Comes from Product service
        public Guid? WarehouseId { get; set; } // Comes from Inventory service
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
