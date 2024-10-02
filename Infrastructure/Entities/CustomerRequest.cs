namespace FirebaseApiMain.Infrastructure.Entities
{
    public class CustomerRequest
    {
        public string ? Flag { get; set; } // Used to identify the operation: create, view_by_id, view_all, update, delete
        public string ? customerId { get; set; } // Unique identifier for the customer
        public string ? firstName { get; set; } // Customer's first name
        public string ? lastName { get; set; } // Customer's last name
        public string ? email { get; set; } // Customer's email address
        public string ? passwordHash { get; set; } // Customer's password (plaintext, should be hashed before saving)
        public string ? phoneNumber { get; set; } // Customer's phone number
        public string ? dateRegistered { get; set; } // Date when the customer registered (ISO 8601 format)
        public string ? customerImageUrl { get; set; } 
        public bool? isActive { get; set; } // Indicates if the customer account is active or inactive
        public string? Addressline1 { get; set; }
        public string? Addressline2 { get; set; }
        public string? Country { get; set; }
        public string? Nearby { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zipCode { get; set; }
        public string? otp { get; set; }
        public string? emailOrPhone { get; set; }

    }


  
}
