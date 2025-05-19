using System;

namespace campus_love_app.domain.entities
{
    public class Message
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public int SenderID { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        
        // Navigation property
        public User? Sender { get; set; }
    }
} 