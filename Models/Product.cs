namespace FirebaseApiMain.Models
{
    public class Product
    {
        public string name { get; set; }
        public string weight { get; set; }
        public decimal price_per_bag { get; set; }
        public decimal price_per_quintal { get; set; }
        public decimal amc { get; set; }
        public string image_url { get; set; }
    }

}
