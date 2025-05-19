using System;
using System.Collections.Generic;

namespace campus_love_app.domain.entities
{
    public class Conversation
    {
        public int ConversationID { get; set; }
        public int User1ID { get; set; }
        public int User2ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastMessageDate { get; set; }
        
        // Navigation properties
        public User? User1 { get; set; }
        public User? User2 { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
        
        // Helper method to get the other user in the conversation
        public int GetOtherUserID(int currentUserID)
        {
            return currentUserID == User1ID ? User2ID : User1ID;
        }
        
        // Helper method to get unread messages count for a user
        public int GetUnreadCount(int userID)
        {
            int count = 0;
            foreach (var message in Messages)
            {
                if (!message.IsRead && message.SenderID != userID)
                {
                    count++;
                }
            }
            return count;
        }
    }
} 