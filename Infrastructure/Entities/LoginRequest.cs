﻿namespace FirebaseApiMain.Infrastructure.Entities
{
    public class LoginRequest
    {
        public string ?Email { get; set; }
        public string ? emailOrPhone { get; set; }
        public string ? Password { get; set; }
    }
}
