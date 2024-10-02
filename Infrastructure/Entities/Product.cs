namespace FirebaseApiMain.Infrastructure.Entities
{
    public class Product
    {
        public string ? Id { get; set; }  // Primary Key
        public string ? name { get; set; }
        public string ? Description { get; set; }

        public string? ShortDescription { get; set; }
        public string? Weight { get; set; }  // Weight of the product
        public string? WeightUnit { get; set; }  // Unit of weight (e.g., kg, grams)

        public int? StockQuantity { get; set; }
        public bool ? IsOutOfStock { get; set; }
        public DateTime? RestockDate { get; set; }
        public string? Discount { get; set; }
        public decimal ? Amount { get; set; }
        public string? image_url { get; set; }
        public string? categoryId { get; set; }
        public double? Rating { get; set; } 
        public int? ReviewCount { get; set; }  

    }
}
