namespace FirebaseApiMain.Infrastructure.Entities
{
    public class Product
    {
        public string ? Id { get; set; }  // Primary Key
        public string name { get; set; }
        public decimal? weight { get; set; }
        public decimal ? no_of_bags { get; set; }
        public decimal?  no_of__quintals { get; set; }
        public decimal ? amc { get; set; }
        public decimal ? Amount { get; set; }
        public string? image_url { get; set; }
        public string? categoryId { get; set; }  // Foreign Key
        //public Category Category { get; set; }  // Navigation property
    }
}
