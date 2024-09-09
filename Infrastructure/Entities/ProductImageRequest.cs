namespace FirebaseApiMain.Infrastructure.Entities
{
    public class ProductImageRequest
    {
        // Flag for CRUD operations: "create", "update", "delete", "view_all", "view_by_id"
        public string ? Flag { get; set; }

        // The product ID (only required for update, delete, view_by_id operations)
        public string ? ProductId { get; set; }

        // Product properties
        public string ? name { get; set; }
        public decimal ? weight { get; set; }
        public decimal ? no_of_bags { get; set; }
        public decimal ? no_of__quintals { get; set; }
        public decimal ?amc { get; set; }
        public decimal ? Amount { get; set; }
        public string ? image_url { get; set; }
        public string ?categoryId { get; set; }

        public IFormFile? imageFile { get; set; }
    }

}
