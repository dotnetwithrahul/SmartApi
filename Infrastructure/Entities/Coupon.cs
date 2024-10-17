namespace FirebaseApiMain.Infrastructure.Entities
{
    public class Coupon
    {
        public string ? CouponId { get; set; }
        public string ? Code { get; set; } // Coupon code
        public decimal? DiscountPercentage { get; set; } // Discount in percentage
        public decimal? MaxDiscountAmount { get; set; } // Max discount allowed
        public decimal? MinimumPurchaseAmount { get; set; } // Minimum purchase amount to apply the coupon
        public DateTime? ExpiryDate { get; set; } // Expiry date for the coupon
        public bool ? IsActive { get; set; } // Coupon active or inactive
        public int?  UsageLimit { get; set; } // Maximum usage limit
        public int?  UsedCount { get; set; } // How many times the coupon was used
    }

}
