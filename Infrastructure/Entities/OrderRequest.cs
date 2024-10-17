namespace FirebaseApiMain.Infrastructure.Entities
{
    public class OrderRequest
    {
        public string ? Flag { get; set; } // create, update_status, view_by_id, view_by_customerId, view_all, cancel
        public string ? OrderId { get; set; }
        public string ? CustomerId { get; set; }
        public string ? Status { get; set; } // For update status
        public string? AdditionalStatus { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
        public List<Item> ? Items { get; set; }
        public string ? CouponCode { get; set; } // For applying discounts
        public string ? PaymentMethod { get; set; } // COD or Online
    }

}
