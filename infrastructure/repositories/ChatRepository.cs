using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly MySqlConnection _connection;
        
        public ChatRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }
        
        public Conversation? GetConversation(int conversationId)
        {
            Conversation? conversation = null;
            
            string query = @"
                SELECT c.*, 
                       u1.FullName as User1Name, 
                       u2.FullName as User2Name
                FROM Conversations c
                JOIN Users u1 ON c.User1ID = u1.UserID
                JOIN Users u2 ON c.User2ID = u2.UserID
                WHERE c.ConversationID = @ConversationId";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ConversationId", conversationId);
                
                using var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    conversation = new Conversation
                    {
                        ConversationID = reader.GetInt32("ConversationID"),
                        User1ID = reader.GetInt32("User1ID"),
                        User2ID = reader.GetInt32("User2ID"),
                        StartDate = reader.GetDateTime("StartDate"),
                        LastMessageDate = reader.GetDateTime("LastMessageDate"),
                        User1 = new User { UserID = reader.GetInt32("User1ID"), FullName = reader.GetString("User1Name") },
                        User2 = new User { UserID = reader.GetInt32("User2ID"), FullName = reader.GetString("User2Name") }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversation: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return conversation;
        }
        
        public Conversation? GetConversationBetweenUsers(int user1Id, int user2Id)
        {
            Conversation? conversation = null;
            
            string query = @"
                SELECT c.*, 
                       u1.FullName as User1Name, 
                       u2.FullName as User2Name
                FROM Conversations c
                JOIN Users u1 ON c.User1ID = u1.UserID
                JOIN Users u2 ON c.User2ID = u2.UserID
                WHERE (c.User1ID = @User1Id AND c.User2ID = @User2Id)
                   OR (c.User1ID = @User2Id AND c.User2ID = @User1Id)";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@User1Id", user1Id);
                cmd.Parameters.AddWithValue("@User2Id", user2Id);
                
                using var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    conversation = new Conversation
                    {
                        ConversationID = reader.GetInt32("ConversationID"),
                        User1ID = reader.GetInt32("User1ID"),
                        User2ID = reader.GetInt32("User2ID"),
                        StartDate = reader.GetDateTime("StartDate"),
                        LastMessageDate = reader.GetDateTime("LastMessageDate"),
                        User1 = new User { UserID = reader.GetInt32("User1ID"), FullName = reader.GetString("User1Name") },
                        User2 = new User { UserID = reader.GetInt32("User2ID"), FullName = reader.GetString("User2Name") }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversation between users: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return conversation;
        }
        
        public List<Conversation> GetUserConversations(int userId)
        {
            var conversations = new List<Conversation>();
            
            string query = @"
                SELECT c.*, 
                       u1.FullName as User1Name, 
                       u2.FullName as User2Name,
                       (SELECT COUNT(*) FROM Messages m 
                        WHERE m.ConversationID = c.ConversationID 
                        AND m.SenderID != @UserId 
                        AND m.IsRead = FALSE) as UnreadCount
                FROM Conversations c
                JOIN Users u1 ON c.User1ID = u1.UserID
                JOIN Users u2 ON c.User2ID = u2.UserID
                WHERE c.User1ID = @UserId OR c.User2ID = @UserId
                ORDER BY c.LastMessageDate DESC";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var conversation = new Conversation
                    {
                        ConversationID = reader.GetInt32("ConversationID"),
                        User1ID = reader.GetInt32("User1ID"),
                        User2ID = reader.GetInt32("User2ID"),
                        StartDate = reader.GetDateTime("StartDate"),
                        LastMessageDate = reader.GetDateTime("LastMessageDate"),
                        User1 = new User { UserID = reader.GetInt32("User1ID"), FullName = reader.GetString("User1Name") },
                        User2 = new User { UserID = reader.GetInt32("User2ID"), FullName = reader.GetString("User2Name") }
                    };
                    
                    conversations.Add(conversation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user conversations: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return conversations;
        }
        
        public int CreateConversation(int user1Id, int user2Id)
        {
            int conversationId = 0;
            
            // Check if a conversation already exists
            var existingConversation = GetConversationBetweenUsers(user1Id, user2Id);
            if (existingConversation != null)
            {
                return existingConversation.ConversationID;
            }
            
            string query = @"
                INSERT INTO Conversations (User1ID, User2ID, StartDate, LastMessageDate)
                VALUES (@User1Id, @User2Id, NOW(), NOW());
                SELECT LAST_INSERT_ID();";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@User1Id", user1Id);
                cmd.Parameters.AddWithValue("@User2Id", user2Id);
                
                conversationId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating conversation: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return conversationId;
        }
        
        public List<Message> GetConversationMessages(int conversationId, int limit = 20, int offset = 0)
        {
            var messages = new List<Message>();
            
            string query = @"
                SELECT m.*, u.FullName as SenderName
                FROM Messages m
                JOIN Users u ON m.SenderID = u.UserID
                WHERE m.ConversationID = @ConversationId
                ORDER BY m.SentDate DESC
                LIMIT @Limit OFFSET @Offset";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ConversationId", conversationId);
                cmd.Parameters.AddWithValue("@Limit", limit);
                cmd.Parameters.AddWithValue("@Offset", offset);
                
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var message = new Message
                    {
                        MessageID = reader.GetInt32("MessageID"),
                        ConversationID = reader.GetInt32("ConversationID"),
                        SenderID = reader.GetInt32("SenderID"),
                        MessageText = reader.GetString("MessageText"),
                        SentDate = reader.GetDateTime("SentDate"),
                        IsRead = reader.GetBoolean("IsRead"),
                        Sender = new User { UserID = reader.GetInt32("SenderID"), FullName = reader.GetString("SenderName") }
                    };
                    
                    messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversation messages: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            // Reverse the list to get messages in chronological order
            messages.Reverse();
            return messages;
        }
        
        public int SendMessage(int conversationId, int senderId, string messageText)
        {
            int messageId = 0;
            
            string query = @"
                INSERT INTO Messages (ConversationID, SenderID, MessageText, SentDate, IsRead)
                VALUES (@ConversationId, @SenderId, @MessageText, NOW(), FALSE);
                
                UPDATE Conversations 
                SET LastMessageDate = NOW() 
                WHERE ConversationID = @ConversationId;
                
                SELECT LAST_INSERT_ID();";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ConversationId", conversationId);
                cmd.Parameters.AddWithValue("@SenderId", senderId);
                cmd.Parameters.AddWithValue("@MessageText", messageText);
                
                messageId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return messageId;
        }
        
        public void MarkMessagesAsRead(int conversationId, int userId)
        {
            string query = @"
                UPDATE Messages 
                SET IsRead = TRUE 
                WHERE ConversationID = @ConversationId 
                AND SenderID != @UserId 
                AND IsRead = FALSE";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@ConversationId", conversationId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking messages as read: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
        
        public int GetUnreadMessagesCount(int userId)
        {
            int count = 0;
            
            string query = @"
                SELECT COUNT(*) FROM Messages m
                JOIN Conversations c ON m.ConversationID = c.ConversationID
                WHERE m.IsRead = FALSE
                AND m.SenderID != @UserId
                AND (c.User1ID = @UserId OR c.User2ID = @UserId)";
                
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread messages count: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return count;
        }
    }
} 