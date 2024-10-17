namespace FirebaseApiMain.Infrastructure.Entities
{
    public class CartRequest
    {
        public string? Flag { get; set; } // For specifying the CRUD operation: "add", "view_by_customer", "remove"
        public string? CartId { get; set; } // For identifying the cart item (used in "remove" operation)
        public string? CustomerId { get; set; } // The ID of the customer adding/removing the product
        public string? ProductId { get; set; } // The ID of the product being added to the cart
        public int? Quantity { get; set; } // The quantity of the product to add to the cart
        public DateTime? DateAdded { get; set; } // The date and time when the product was added to the cart
    }   
}
