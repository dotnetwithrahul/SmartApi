namespace FirebaseApiMain.Infrastructure.Entities
{
    public class CouponRequest
    {
        public string? Flag { get; set; } // create, view_by_id, view_all, update, delete
        public string? CouponId { get; set; } // For view_by_id, update, and delete
        public string? Code { get; set; } // Coupon code
        public decimal? DiscountPercentage { get; set; } // Discount in percentage
        public decimal? MaxDiscountAmount { get; set; } // Max discount allowed
        public decimal? MinimumPurchaseAmount { get; set; } // Minimum purchase amount to apply the coupon
        public DateTime? ExpiryDate { get; set; } // Expiry date
        public bool? IsActive { get; set; } // Active status
        public int? UsageLimit { get; set; } // Usage limit
    }

}
