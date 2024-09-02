namespace FirebaseApiMain.Infrastructure.Entities
{
    public class CategoryRequest
    {
        // Flag for CRUD operations: "create", "update", "delete", "view_all", "view_by_id"
        public string? Flag { get; set; }

        // The product ID (only required for update, delete, view_by_id operations)
        public string? CategoryId { get; set; }

        public string? name { get; set; }
        public string? image_url { get; set; }
        //public IFormFile ? imageFile { get; set; }
    }
}
