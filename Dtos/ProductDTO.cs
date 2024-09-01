namespace FirebaseApiMain.Dtos
{
    public class ProductDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Weight { get; set; }
        public decimal PricePerBag { get; set; }
        public decimal PricePerQuintal { get; set; }
        public decimal Amc { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }  
    }

}
