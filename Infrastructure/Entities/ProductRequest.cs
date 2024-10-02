namespace FirebaseApiMain.Infrastructure.Entities
{
    public class ProductRequest
    {
        // Flag for CRUD operations: "create", "update", "delete", "view_all", "view_by_id"
        public string ? Flag { get; set; }

        // The product ID (only required for update, delete, view_by_id operations)
        public string ? ProductId { get; set; }

        // Product properties
        public string? Id { get; set; }  // Primary Key
        public string? name { get; set; }
        public string? Description { get; set; }

        public string? ShortDescription { get; set; }
        public string? Weight { get; set; }  // Weight of the product
        public string? WeightUnit { get; set; }  // Unit of weight (e.g., kg, grams)

        public int? StockQuantity { get; set; }
        public bool? IsOutOfStock { get; set; }
        public DateTime? RestockDate { get; set; }
        public string? Discount { get; set; }
        public decimal? Amount { get; set; }
        public string? image_url { get; set; }
        public string? categoryId { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }

    }

}
