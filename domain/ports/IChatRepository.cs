using System.Collections.Generic;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface IChatRepository
    {
        // Conversation methods
        Conversation? GetConversation(int conversationId);
        Conversation? GetConversationBetweenUsers(int user1Id, int user2Id);
        List<Conversation> GetUserConversations(int userId);
        int CreateConversation(int user1Id, int user2Id);
        
        // Message methods
        List<Message> GetConversationMessages(int conversationId, int limit = 20, int offset = 0);
        int SendMessage(int conversationId, int senderId, string messageText);
        void MarkMessagesAsRead(int conversationId, int userId);
        int GetUnreadMessagesCount(int userId);
    }
} 