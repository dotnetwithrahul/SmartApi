namespace FirebaseApiMain.Infrastructure.Entities
{
    public class Category
    {
        //public string Id { get; set; }  // Primary Key
        public string name { get; set; }
        public string image_url { get; set; }

        public IFormFile? imageFile { get; set; }
        
        //public ICollection<Product> Products { get; set; }  // Navigation property
    }
}
