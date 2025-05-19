using System;

namespace campus_love_app.domain.entities
{
    public class UserCredits
    {
        public int UserID { get; set; }
        public int CreditsRemaining { get; set; }
        public DateTime LastResetDate { get; set; }

        // Constructor
        public UserCredits(int userId, int creditsRemaining = 10, DateTime? lastResetDate = null)
        {
            UserID = userId;
            CreditsRemaining = creditsRemaining;
            LastResetDate = lastResetDate ?? DateTime.Today;
        }
    }
} 