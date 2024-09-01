namespace FirebaseApiMain.Infrastructure.Entities
{
    public class PaymentRequest
    {

        public string CustomerId { get; set; }
        public Dictionary<string, int> Products { get; set; }

    }
}
