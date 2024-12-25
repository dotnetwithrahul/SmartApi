namespace FirebaseApiMain.Infrastructure.Auth.Models
{
    public class AdminUsers
    {
        public string? Flag { get; set; } // Used to identify the operation: create, view_by_id, view_all, update, delete
        public string? adminId { get; set; } // Unique identifier for the customer
        public string? firstName { get; set; } // Customer's first name
        public string? lastName { get; set; } // Customer's last name
        public string? email { get; set; } // Customer's email address
        public string? Role { get; set; } // Customer's email address
        public string? emailOrPhone { get; set; } // Customer's email address
        public string? passwordHash { get; set; } // Customer's password (plaintext, should be hashed before saving)
        public string? phoneNumber { get; set; } // Customer's phone number
        public string? dateRegistered { get; set; } // Date when the customer registered (ISO 8601 format)

    }
}
