namespace FirebaseApiMain.Infrastructure.Entities
{
    public class Order
    {
        public string ? OrderId { get; set; }
        public string ? CustomerId { get; set; }
        public DateTime ? OrderPlacedDate { get; set; }
        public DateTime? DispatchedDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string ? Status { get; set; } // e.g. Placed, Dispatched, OutForDelivery, Delivered, Cancelled
        public string ? AdditionalStatus { get; set; } // e.g. Placed, Dispatched, OutForDelivery, Delivered, Cancelled

        public List<Item>? Items { get; set; }
        public decimal ? SubTotal { get; set; }
        public decimal ? DeliveryCharges { get; set; }
        public decimal ? Tax { get; set; }
        public decimal ?  TotalAmount { get; set; }
        public string?  PaymentMethod { get; set; } // COD, Online
        public string ? CouponCode { get; set; }
        public decimal ? Discount { get; set; }

        public CustomerRequest CustomerDetails { get; set; } // Add this line
    }

    public class Item
    {
        public string ? ProductId { get; set; }
        public string ? ProductName { get; set; }
        public int ?  Quantity { get; set; }
        public decimal  ? UnitPrice { get; set; }  
        public decimal ?  TotalPrice { get; set; }
        public ProductDetails ? ProductDetails { get; set; }
    }




}
