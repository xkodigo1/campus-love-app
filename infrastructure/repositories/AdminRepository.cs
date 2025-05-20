using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace campus_love_app.infrastructure.repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly MySqlConnection _connection;

        public AdminRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        public Administrator GetAdminByUsername(string username)
        {
            string query = "SELECT * FROM Administrators WHERE Username = @Username";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Username", username);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Administrator
                    {
                        AdminID = reader.GetInt32("AdminID"),
                        Username = reader.GetString("Username"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : reader.GetDateTime("LastLoginDate"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    };
                }

                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public Administrator GetAdminByEmail(string email)
        {
            string query = "SELECT * FROM Administrators WHERE Email = @Email";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Email", email);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Administrator
                    {
                        AdminID = reader.GetInt32("AdminID"),
                        Username = reader.GetString("Username"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : reader.GetDateTime("LastLoginDate"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    };
                }

                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool ValidateAdmin(string username, string password)
        {
            var admin = GetAdminByUsername(username);
            if (admin == null || !admin.IsActive)
                return false;

            // Para desarrollo/pruebas, permitir iniciar sesión con 'admin123'
            if (username == "admin" && password == "admin123")
            {
                return true;
            }

            // La verificación normal con hash (que actualmente no está funcionando correctamente)
            return admin.PasswordHash == password || VerifyPassword(password, admin.PasswordHash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // This is a simplified method
            // In a real system, you would use a proper hashing library with salts
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString() == storedHash;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            string query = @"
                SELECT u.*, g.GenderName, c.CareerName, o.OrientationName 
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN SexualOrientations o ON u.OrientationID = o.OrientationID
                ORDER BY u.UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var user = new User
                    {
                        UserID = reader.GetInt32("UserID"),
                        FullName = reader.GetString("FullName"),
                        Age = reader.GetInt32("Age"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        OrientationID = reader.GetInt32("OrientationID"),
                        ProfilePhrase = reader.GetString("ProfilePhrase"),
                        MinPreferredAge = reader.GetInt32("MinPreferredAge"),
                        MaxPreferredAge = reader.GetInt32("MaxPreferredAge"),
                        IsVerified = reader.GetBoolean("IsVerified"),
                        // Extended profile fields
                        ExtendedDescription = !reader.IsDBNull(reader.GetOrdinal("ExtendedDescription")) 
                            ? reader.GetString("ExtendedDescription") : string.Empty,
                        Hobbies = !reader.IsDBNull(reader.GetOrdinal("Hobbies")) 
                            ? reader.GetString("Hobbies") : string.Empty,
                        FavoriteBooks = !reader.IsDBNull(reader.GetOrdinal("FavoriteBooks")) 
                            ? reader.GetString("FavoriteBooks") : string.Empty,
                        FavoriteMovies = !reader.IsDBNull(reader.GetOrdinal("FavoriteMovies")) 
                            ? reader.GetString("FavoriteMovies") : string.Empty,
                        FavoriteMusic = !reader.IsDBNull(reader.GetOrdinal("FavoriteMusic")) 
                            ? reader.GetString("FavoriteMusic") : string.Empty,
                        HasEnrichedProfile = !reader.IsDBNull(reader.GetOrdinal("HasEnrichedProfile")) 
                            ? reader.GetBoolean("HasEnrichedProfile") : false
                    };

                    users.Add(user);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return users;
        }

        public User GetUserById(int userId)
        {
            string query = @"
                SELECT u.*, g.GenderName, c.CareerName, o.OrientationName 
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN SexualOrientations o ON u.OrientationID = o.OrientationID
                WHERE u.UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        UserID = reader.GetInt32("UserID"),
                        FullName = reader.GetString("FullName"),
                        Age = reader.GetInt32("Age"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        OrientationID = reader.GetInt32("OrientationID"),
                        ProfilePhrase = reader.GetString("ProfilePhrase"),
                        MinPreferredAge = reader.GetInt32("MinPreferredAge"),
                        MaxPreferredAge = reader.GetInt32("MaxPreferredAge"),
                        IsVerified = reader.GetBoolean("IsVerified"),
                        // Extended profile fields
                        ExtendedDescription = !reader.IsDBNull(reader.GetOrdinal("ExtendedDescription")) 
                            ? reader.GetString("ExtendedDescription") : string.Empty,
                        Hobbies = !reader.IsDBNull(reader.GetOrdinal("Hobbies")) 
                            ? reader.GetString("Hobbies") : string.Empty,
                        FavoriteBooks = !reader.IsDBNull(reader.GetOrdinal("FavoriteBooks")) 
                            ? reader.GetString("FavoriteBooks") : string.Empty,
                        FavoriteMovies = !reader.IsDBNull(reader.GetOrdinal("FavoriteMovies")) 
                            ? reader.GetString("FavoriteMovies") : string.Empty,
                        FavoriteMusic = !reader.IsDBNull(reader.GetOrdinal("FavoriteMusic")) 
                            ? reader.GetString("FavoriteMusic") : string.Empty,
                        HasEnrichedProfile = !reader.IsDBNull(reader.GetOrdinal("HasEnrichedProfile")) 
                            ? reader.GetBoolean("HasEnrichedProfile") : false
                    };
                }

                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool VerifyUser(int userId)
        {
            string query = "UPDATE Users SET IsVerified = TRUE WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool BanUser(int userId)
        {
            string query = "UPDATE UserAccounts SET IsActive = FALSE WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool UnbanUser(int userId)
        {
            string query = "UPDATE UserAccounts SET IsActive = TRUE WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool DeleteUser(int userId)
        {
            // Note: In a real-world application, you might want to backup the user data
            // or mark them as deleted instead of actually deleting them
            string query = "DELETE FROM Users WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // First, delete associated records in dependent tables
                DeleteUserDependentRecords(userId);

                // Then delete the user
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        private void DeleteUserDependentRecords(int userId)
        {
            // This is a simplified version - in a real application,
            // you would need to handle all related records properly
            var tables = new[]
            {
                "UserAccounts",
                "UserInterests",
                "Interactions",
                "Matches",
                "Messages",
                "Conversations",
                "user_credits",
                "DailyCredits"
            };

            foreach (var table in tables)
            {
                try
                {
                    // Handle tables with different foreign key structures
                    string query = table switch
                    {
                        "Matches" => $"DELETE FROM {table} WHERE User1ID = @UserID OR User2ID = @UserID",
                        "Interactions" => $"DELETE FROM {table} WHERE FromUserID = @UserID OR ToUserID = @UserID",
                        "Conversations" => $"DELETE FROM {table} WHERE User1ID = @UserID OR User2ID = @UserID",
                        "Messages" => $"DELETE FROM {table} WHERE SenderID = @UserID",
                        "user_credits" => $"DELETE FROM {table} WHERE user_id = @UserID",
                        _ => $"DELETE FROM {table} WHERE UserID = @UserID"
                    };

                    using var cmd = new MySqlCommand(query, _connection);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete from {table}: {ex.Message}");
                }
            }
        }

        public List<User> SearchUsers(string searchTerm)
        {
            var users = new List<User>();
            string query = @"
                SELECT u.*, g.GenderName, c.CareerName, o.OrientationName 
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN SexualOrientations o ON u.OrientationID = o.OrientationID
                WHERE u.FullName LIKE @SearchTerm 
                OR u.ProfilePhrase LIKE @SearchTerm
                OR c.CareerName LIKE @SearchTerm
                OR EXISTS (SELECT 1 FROM UserAccounts ua WHERE ua.UserID = u.UserID AND ua.Username LIKE @SearchTerm)
                OR EXISTS (SELECT 1 FROM UserAccounts ua WHERE ua.UserID = u.UserID AND ua.Email LIKE @SearchTerm)
                ORDER BY u.UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User
                    {
                        UserID = reader.GetInt32("UserID"),
                        FullName = reader.GetString("FullName"),
                        Age = reader.GetInt32("Age"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        OrientationID = reader.GetInt32("OrientationID"),
                        ProfilePhrase = reader.GetString("ProfilePhrase"),
                        MinPreferredAge = reader.GetInt32("MinPreferredAge"),
                        MaxPreferredAge = reader.GetInt32("MaxPreferredAge"),
                        IsVerified = reader.GetBoolean("IsVerified"),
                        // Extended profile fields
                        ExtendedDescription = !reader.IsDBNull(reader.GetOrdinal("ExtendedDescription")) 
                            ? reader.GetString("ExtendedDescription") : string.Empty,
                        Hobbies = !reader.IsDBNull(reader.GetOrdinal("Hobbies")) 
                            ? reader.GetString("Hobbies") : string.Empty,
                        FavoriteBooks = !reader.IsDBNull(reader.GetOrdinal("FavoriteBooks")) 
                            ? reader.GetString("FavoriteBooks") : string.Empty,
                        FavoriteMovies = !reader.IsDBNull(reader.GetOrdinal("FavoriteMovies")) 
                            ? reader.GetString("FavoriteMovies") : string.Empty,
                        FavoriteMusic = !reader.IsDBNull(reader.GetOrdinal("FavoriteMusic")) 
                            ? reader.GetString("FavoriteMusic") : string.Empty,
                        HasEnrichedProfile = !reader.IsDBNull(reader.GetOrdinal("HasEnrichedProfile")) 
                            ? reader.GetBoolean("HasEnrichedProfile") : false
                    };

                    users.Add(user);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return users;
        }

        #region Statistics Methods

        public Dictionary<string, int> GetUserStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Total users
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users", _connection))
                {
                    stats["TotalUsers"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // New users last 7 days
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE RegistrationDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)", _connection))
                {
                    stats["NewUsersLast7Days"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // New users last 30 days
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE RegistrationDate >= DATE_SUB(NOW(), INTERVAL 30 DAY)", _connection))
                {
                    stats["NewUsersLast30Days"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Verified users
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE IsVerified = TRUE", _connection))
                {
                    stats["VerifiedUsers"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Enriched profiles
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE HasEnrichedProfile = TRUE", _connection))
                {
                    stats["EnrichedProfiles"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Active users (users with accounts marked as active)
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM UserAccounts WHERE IsActive = TRUE", _connection))
                {
                    stats["ActiveUsers"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Calculate percentages
                if (stats["TotalUsers"] > 0)
                {
                    stats["VerifiedPercentage"] = (int)((double)stats["VerifiedUsers"] / stats["TotalUsers"] * 100);
                    stats["EnrichedProfilePercentage"] = (int)((double)stats["EnrichedProfiles"] / stats["TotalUsers"] * 100);
                    stats["ActivePercentage"] = (int)((double)stats["ActiveUsers"] / stats["TotalUsers"] * 100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user statistics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }

        public Dictionary<string, Dictionary<string, int>> GetUserDemographics()
        {
            var demographics = new Dictionary<string, Dictionary<string, int>>();
            demographics["Gender"] = new Dictionary<string, int>();
            demographics["Orientation"] = new Dictionary<string, int>();
            demographics["Career"] = new Dictionary<string, int>();
            demographics["AgeGroup"] = new Dictionary<string, int>();
            demographics["Location"] = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Gender distribution
                using (var cmd = new MySqlCommand(@"
                    SELECT g.GenderName, COUNT(*) as Count 
                    FROM Users u 
                    JOIN Genders g ON u.GenderID = g.GenderID 
                    GROUP BY g.GenderName", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        demographics["Gender"][reader.GetString("GenderName")] = reader.GetInt32("Count");
                    }
                }
                
                // Sexual orientation distribution
                using (var cmd = new MySqlCommand(@"
                    SELECT o.OrientationName, COUNT(*) as Count 
                    FROM Users u 
                    JOIN SexualOrientations o ON u.OrientationID = o.OrientationID 
                    GROUP BY o.OrientationName", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        demographics["Orientation"][reader.GetString("OrientationName")] = reader.GetInt32("Count");
                    }
                }
                
                // Career distribution
                using (var cmd = new MySqlCommand(@"
                    SELECT c.CareerName, COUNT(*) as Count 
                    FROM Users u 
                    JOIN Careers c ON u.CareerID = c.CareerID 
                    GROUP BY c.CareerName", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        demographics["Career"][reader.GetString("CareerName")] = reader.GetInt32("Count");
                    }
                }
                
                // Age group distribution
                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        CASE 
                            WHEN Age < 20 THEN '18-19' 
                            WHEN Age BETWEEN 20 AND 24 THEN '20-24' 
                            WHEN Age BETWEEN 25 AND 29 THEN '25-29' 
                            WHEN Age BETWEEN 30 AND 34 THEN '30-34' 
                            WHEN Age >= 35 THEN '35+' 
                        END AS AgeGroup, 
                        COUNT(*) as Count 
                    FROM Users 
                    GROUP BY AgeGroup", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        demographics["AgeGroup"][reader.GetString("AgeGroup")] = reader.GetInt32("Count");
                    }
                }
                
                // Location distribution (top 5 cities)
                using (var cmd = new MySqlCommand(@"
                    SELECT ct.CityName, COUNT(*) as Count 
                    FROM Users u 
                    JOIN Cities ct ON u.CityID = ct.CityID 
                    GROUP BY ct.CityName 
                    ORDER BY Count DESC 
                    LIMIT 5", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        demographics["Location"][reader.GetString("CityName")] = reader.GetInt32("Count");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user demographics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return demographics;
        }

        public Dictionary<string, int> GetInteractionStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Total likes
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM Interactions 
                    WHERE InteractionType = 'LIKE'", _connection))
                {
                    stats["TotalLikes"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Total dislikes
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM Interactions 
                    WHERE InteractionType = 'DISLIKE'", _connection))
                {
                    stats["TotalDislikes"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Total matches
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Matches", _connection))
                {
                    stats["TotalMatches"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Daily likes average
                using (var cmd = new MySqlCommand(@"
                    SELECT AVG(daily_likes) FROM (
                        SELECT DATE(InteractionDate) as day, COUNT(*) as daily_likes
                        FROM Interactions
                        WHERE InteractionType = 'LIKE'
                        GROUP BY day
                    ) as daily_counts", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    stats["DailyLikesAverage"] = result != DBNull.Value ? (int)Convert.ToDouble(result) : 0;
                }
                
                // Match rate (%)
                if (stats["TotalLikes"] > 0)
                {
                    stats["MatchRate"] = (int)((double)stats["TotalMatches"] * 2 / stats["TotalLikes"] * 100);
                }
                
                // Most popular profile (most likes received)
                using (var cmd = new MySqlCommand(@"
                    SELECT ToUserID, COUNT(*) as LikeCount
                    FROM Interactions
                    WHERE InteractionType = 'LIKE'
                    GROUP BY ToUserID
                    ORDER BY LikeCount DESC
                    LIMIT 1", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        stats["MostPopularUserID"] = reader.GetInt32("ToUserID");
                        stats["MostPopularUserLikes"] = reader.GetInt32("LikeCount");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting interaction statistics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }

        public Dictionary<string, int> GetCommunicationStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Total conversations
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Conversations", _connection))
                {
                    stats["TotalConversations"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Total messages
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Messages", _connection))
                {
                    stats["TotalMessages"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Average messages per conversation
                if (stats["TotalConversations"] > 0)
                {
                    stats["AverageMessagesPerConversation"] = stats["TotalMessages"] / stats["TotalConversations"];
                }
                
                // Active conversations (with messages in the last 7 days)
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(DISTINCT ConversationID) FROM Messages
                    WHERE SentDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)", _connection))
                {
                    stats["ActiveConversations"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Unread messages
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM Messages
                    WHERE IsRead = FALSE", _connection))
                {
                    stats["UnreadMessages"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Long conversations (more than 10 messages)
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM (
                        SELECT ConversationID, COUNT(*) as MessageCount
                        FROM Messages
                        GROUP BY ConversationID
                        HAVING MessageCount > 10
                    ) as long_convs", _connection))
                {
                    stats["LongConversations"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting communication statistics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }

        public Dictionary<string, int> GetUsageStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Activity by hour (most active hour)
                using (var cmd = new MySqlCommand(@"
                    SELECT HOUR(InteractionDate) as Hour, COUNT(*) as ActivityCount
                    FROM Interactions
                    GROUP BY Hour
                    ORDER BY ActivityCount DESC
                    LIMIT 1", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        stats["MostActiveHour"] = reader.GetInt32("Hour");
                        stats["MostActiveHourCount"] = reader.GetInt32("ActivityCount");
                    }
                }
                
                // Activity by day of week (most active day)
                using (var cmd = new MySqlCommand(@"
                    SELECT DAYOFWEEK(InteractionDate) as DayOfWeek, COUNT(*) as ActivityCount
                    FROM Interactions
                    GROUP BY DayOfWeek
                    ORDER BY ActivityCount DESC
                    LIMIT 1", _connection))
                {
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        stats["MostActiveDay"] = reader.GetInt32("DayOfWeek");
                        stats["MostActiveDayCount"] = reader.GetInt32("ActivityCount");
                    }
                }
                
                // Activity last 24 hours
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM (
                        SELECT FromUserID FROM Interactions WHERE InteractionDate >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
                        UNION
                        SELECT SenderID FROM Messages WHERE SentDate >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
                    ) as active_users", _connection))
                {
                    stats["ActiveUsers24h"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Activity last 7 days
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM (
                        SELECT FromUserID FROM Interactions WHERE InteractionDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)
                        UNION
                        SELECT SenderID FROM Messages WHERE SentDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)
                    ) as active_users", _connection))
                {
                    stats["ActiveUsers7d"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Activity last 30 days
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM (
                        SELECT FromUserID FROM Interactions WHERE InteractionDate >= DATE_SUB(NOW(), INTERVAL 30 DAY)
                        UNION
                        SELECT SenderID FROM Messages WHERE SentDate >= DATE_SUB(NOW(), INTERVAL 30 DAY)
                    ) as active_users", _connection))
                {
                    stats["ActiveUsers30d"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting usage statistics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }

        public Dictionary<string, int> GetCreditStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Total credits available (sum of remaining credits)
                using (var cmd = new MySqlCommand(@"
                    SELECT SUM(credits_remaining) FROM user_credits", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    stats["TotalCreditsAvailable"] = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
                
                // Users with max credits
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM user_credits
                    WHERE credits_remaining = 10", _connection))
                {
                    stats["UsersWithMaxCredits"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Users with no credits
                using (var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM user_credits
                    WHERE credits_remaining = 0", _connection))
                {
                    stats["UsersWithNoCredits"] = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                // Average credits per user
                using (var cmd = new MySqlCommand(@"
                    SELECT AVG(credits_remaining) FROM user_credits", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    stats["AverageCreditsPerUser"] = result != DBNull.Value ? (int)Convert.ToDouble(result) : 0;
                }
                
                // Credits used today
                using (var cmd = new MySqlCommand(@"
                    SELECT SUM(LikesUsed) FROM DailyCredits
                    WHERE CreditDate = CURDATE()", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    stats["CreditsUsedToday"] = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
                
                // Average credits used per day (last 7 days)
                using (var cmd = new MySqlCommand(@"
                    SELECT AVG(daily_usage) FROM (
                        SELECT CreditDate, SUM(LikesUsed) as daily_usage
                        FROM DailyCredits
                        WHERE CreditDate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)
                        GROUP BY CreditDate
                    ) as daily_stats", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    stats["AverageCreditsPerDay"] = result != DBNull.Value ? (int)Convert.ToDouble(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting credit statistics: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }

        public Dictionary<string, int> GetModerationStatistics()
        {
            // For demonstration purposes, as we don't have moderation tables yet
            var stats = new Dictionary<string, int>
            {
                { "TotalReportedUsers", 5 },
                { "UnverifiedUsers", 3 },
                { "BannedUsers", 2 },
                { "PendingReports", 1 },
                { "ReportsProcessedLast7Days", 4 },
                { "AverageReportResolutionTime", 8 } // hours
            };
            
            return stats;
        }

        #endregion
    }
} 