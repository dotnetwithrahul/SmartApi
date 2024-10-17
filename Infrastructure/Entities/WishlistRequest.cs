namespace FirebaseApiMain.Infrastructure.Entities
{
    public class WishlistRequest
    {
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public string? WishlistId { get; set; } // Used for view_by_id and delete
        public DateTime? DateAdded { get; set; } // Optional date field for when the item was wishlisted
        public string? Flag { get; set; }
    }
}
