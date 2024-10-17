namespace FirebaseApiMain.Infrastructure.Entities
{
    public class ProductDetails
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ShortDescription { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Rating { get; set; }
        public string ? ImageUrl { get; set; }
        public bool ? IsOutOfStock { get; set; }
    }

}
