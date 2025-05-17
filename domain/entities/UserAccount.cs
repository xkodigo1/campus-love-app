using System;

namespace campus_love_app.domain.entities
{
    public class UserAccount
    {
        public int AccountID { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }
} 