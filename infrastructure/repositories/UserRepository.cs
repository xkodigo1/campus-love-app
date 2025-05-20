using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MySqlConnection _connection;

        public UserRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        public void RegisterUser(User user)
        {
            string query = @"INSERT INTO Users (FullName, Age, GenderID, CareerID, CityID, OrientationID, ProfilePhrase, MinPreferredAge, MaxPreferredAge, IsVerified) 
                            VALUES (@FullName, @Age, @GenderID, @CareerID, @CityID, @OrientationID, @ProfilePhrase, @MinPreferredAge, @MaxPreferredAge, @IsVerified);
                            SELECT LAST_INSERT_ID();";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Age", user.Age);
                cmd.Parameters.AddWithValue("@GenderID", user.GenderID);
                cmd.Parameters.AddWithValue("@CareerID", user.CareerID);
                cmd.Parameters.AddWithValue("@CityID", user.CityID);
                cmd.Parameters.AddWithValue("@OrientationID", user.OrientationID);
                cmd.Parameters.AddWithValue("@ProfilePhrase", user.ProfilePhrase);
                cmd.Parameters.AddWithValue("@MinPreferredAge", user.MinPreferredAge);
                cmd.Parameters.AddWithValue("@MaxPreferredAge", user.MaxPreferredAge);
                cmd.Parameters.AddWithValue("@IsVerified", user.IsVerified);

                // Get the inserted ID and assign it to the user
                int userId = Convert.ToInt32(cmd.ExecuteScalar());
                user.UserID = userId;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public List<User> GetAvailableProfiles(int userId)
        {
            // Get the user's preferences
            User currentUser = GetUserById(userId);
            if (currentUser == null) return new List<User>();

            // Query to get available profiles based on sexual orientation and excluding users
            // that the current user has already interacted with (liked or disliked)
            string query = @"
                SELECT u.*, g.GenderName AS GenderDesc, c.CareerName AS CareerName, city.CityName AS CityName, 
                       so.OrientationName AS OrientationDesc
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN Cities city ON u.CityID = city.CityID
                JOIN SexualOrientations so ON u.OrientationID = so.OrientationID
                WHERE u.UserID <> @UserId
                AND u.UserID NOT IN (
                    SELECT ToUserID FROM Interactions WHERE FromUserID = @UserId
                )";

            // Add filtering based on sexual orientation
            if (currentUser.OrientationID == 1) // Straight
            {
                if (currentUser.GenderID == 1) // Male looking for Female
                    query += " AND u.GenderID = 2";
                else // Female looking for Male
                    query += " AND u.GenderID = 1";
            }
            else if (currentUser.OrientationID == 2) // Gay
            {
                if (currentUser.GenderID == 1) // Male looking for Male
                    query += " AND u.GenderID = 1";
                else // Female looking for Female
                    query += " AND u.GenderID = 2";
            }
            // For bisexual users, no gender filter needed

            var users = new List<User>();

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
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
                        // Nuevos campos de enriquecimiento de perfil
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
                        InstagramProfile = !reader.IsDBNull(reader.GetOrdinal("InstagramProfile")) 
                            ? reader.GetString("InstagramProfile") : string.Empty,
                        TwitterProfile = !reader.IsDBNull(reader.GetOrdinal("TwitterProfile")) 
                            ? reader.GetString("TwitterProfile") : string.Empty,
                        LinkedInProfile = !reader.IsDBNull(reader.GetOrdinal("LinkedInProfile")) 
                            ? reader.GetString("LinkedInProfile") : string.Empty,
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

        public void LikeUser(int fromUserId, int toUserId)
        {
            try
            {
                // First check if the user has credits available
                ResetCreditsIfNeeded(fromUserId);
                int creditsRemaining = GetRemainingCredits(fromUserId);
                
                if (creditsRemaining <= 0)
                {
                    // User has no credits left
                    Console.WriteLine($"DEBUG: User {fromUserId} has no credits left to give likes");
                    throw new Exception("No credits remaining to give likes today");
                }
                
                // Use one credit
                if (!UseCredit(fromUserId))
                {
                    throw new Exception("Failed to use credit");
                }

                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // Register the like interaction
                string interactionQuery = "INSERT INTO Interactions (FromUserID, ToUserID, InteractionType, InteractionDate) VALUES (@FromUserID, @ToUserID, 'LIKE', NOW())";
                using var interactionCmd = new MySqlCommand(interactionQuery, _connection);
                interactionCmd.Parameters.AddWithValue("@FromUserID", fromUserId);
                interactionCmd.Parameters.AddWithValue("@ToUserID", toUserId);
                interactionCmd.ExecuteNonQuery();
                
                Console.WriteLine($"DEBUG: Added like interaction from {fromUserId} to {toUserId}");

                // Check if there's a match (if the other person has already liked this user)
                string matchCheckQuery = "SELECT COUNT(*) FROM Interactions WHERE FromUserID = @ToUserID AND ToUserID = @FromUserID AND InteractionType = 'LIKE'";
                using var matchCheckCmd = new MySqlCommand(matchCheckQuery, _connection);
                matchCheckCmd.Parameters.AddWithValue("@FromUserID", fromUserId);
                matchCheckCmd.Parameters.AddWithValue("@ToUserID", toUserId);
                
                int hasMatch = Convert.ToInt32(matchCheckCmd.ExecuteScalar());
                Console.WriteLine($"DEBUG: Match check result: {hasMatch}");
                
                if (hasMatch > 0)
                {
                    // First check if this match already exists to avoid duplicates
                    string checkExistingMatch = @"
                        SELECT COUNT(*) FROM Matches 
                        WHERE (User1ID = @User1ID AND User2ID = @User2ID) 
                           OR (User1ID = @User2ID AND User2ID = @User1ID)";
                    using var checkExistingCmd = new MySqlCommand(checkExistingMatch, _connection);
                    checkExistingCmd.Parameters.AddWithValue("@User1ID", fromUserId);
                    checkExistingCmd.Parameters.AddWithValue("@User2ID", toUserId);
                    
                    int existingMatches = Convert.ToInt32(checkExistingCmd.ExecuteScalar());
                    if (existingMatches > 0)
                    {
                        Console.WriteLine($"DEBUG: Match between {fromUserId} and {toUserId} already exists");
                        return; // Match already exists, no need to create it again
                    }
                    
                    // It's a match! Create entry in Matches table
                    string createMatchQuery = "INSERT INTO Matches (User1ID, User2ID, MatchDate) VALUES (@User1ID, @User2ID, NOW())";
                    using var createMatchCmd = new MySqlCommand(createMatchQuery, _connection);
                    createMatchCmd.Parameters.AddWithValue("@User1ID", fromUserId);
                    createMatchCmd.Parameters.AddWithValue("@User2ID", toUserId);
                    
                    try
                    {
                        createMatchCmd.ExecuteNonQuery();
                        Console.WriteLine($"DEBUG: Created new match between {fromUserId} and {toUserId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG ERROR: Failed to create match: {ex.Message}");
                    }
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public List<User> GetMatches(int userId)
        {
            var matches = new List<User>();
            
            string query = @"
                SELECT u.* 
                FROM Users u
                WHERE u.UserID IN (
                    SELECT User2ID FROM Matches WHERE User1ID = @UserId
                    UNION
                    SELECT User1ID FROM Matches WHERE User2ID = @UserId
                )";
                
            Console.WriteLine($"DEBUG: Getting matches for user {userId}");

            // Add direct count check for debugging
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                string countQuery = @"
                    SELECT COUNT(*) 
                    FROM Matches 
                    WHERE User1ID = @UserId OR User2ID = @UserId";
                    
                using var countCmd = new MySqlCommand(countQuery, _connection);
                countCmd.Parameters.AddWithValue("@UserId", userId);
                int matchCount = Convert.ToInt32(countCmd.ExecuteScalar());
                Console.WriteLine($"DEBUG: Direct database check shows {matchCount} matches for user {userId}");
                
                // Now continue with the original query
                _connection.Close();
                _connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG ERROR in count check: {ex.Message}");
            }

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
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
                        IsVerified = reader.GetBoolean("IsVerified")
                    };
                    
                    matches.Add(user);
                    Console.WriteLine($"DEBUG: Found match with user {user.UserID} ({user.FullName})");
                }
                
                Console.WriteLine($"DEBUG: Found {matches.Count} matches for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG ERROR in GetMatches: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return matches;
        }

        // Get a user by ID
        public User? GetUserById(int userId)
        {
            User user = null;
            
            string query = @"SELECT * FROM Users WHERE UserID = @UserId";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                using var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    user = new User
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
                        // Nuevos campos de enriquecimiento de perfil
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
                        InstagramProfile = !reader.IsDBNull(reader.GetOrdinal("InstagramProfile")) 
                            ? reader.GetString("InstagramProfile") : string.Empty,
                        TwitterProfile = !reader.IsDBNull(reader.GetOrdinal("TwitterProfile")) 
                            ? reader.GetString("TwitterProfile") : string.Empty,
                        LinkedInProfile = !reader.IsDBNull(reader.GetOrdinal("LinkedInProfile")) 
                            ? reader.GetString("LinkedInProfile") : string.Empty,
                        HasEnrichedProfile = !reader.IsDBNull(reader.GetOrdinal("HasEnrichedProfile")) 
                            ? reader.GetBoolean("HasEnrichedProfile") : false
                    };
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return user;
        }
        
        // Debug method to check if matches exist in the database
        public List<(int User1ID, int User2ID)> GetAllMatchesDebug()
        {
            var matchesList = new List<(int User1ID, int User2ID)>();
            
            string query = "SELECT User1ID, User2ID FROM Matches";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                
                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    int user1Id = reader.GetInt32("User1ID");
                    int user2Id = reader.GetInt32("User2ID");
                    matchesList.Add((user1Id, user2Id));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking matches: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return matchesList;
        }
        
        // Debug method to check interactions
        public List<(int FromUserID, int ToUserID, string Type)> GetAllInteractionsDebug()
        {
            var interactionsList = new List<(int FromUserID, int ToUserID, string Type)>();
            
            string query = "SELECT FromUserID, ToUserID, InteractionType FROM Interactions";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                
                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    int fromUserId = reader.GetInt32("FromUserID");
                    int toUserId = reader.GetInt32("ToUserID");
                    string type = reader.GetString("InteractionType");
                    interactionsList.Add((fromUserId, toUserId, type));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking interactions: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return interactionsList;
        }

        public Dictionary<string, string> GetUserBasicStatistics(int userId)
        {
            var stats = new Dictionary<string, string>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                // Total de likes dados
                string likesGivenQuery = @"
                    SELECT COUNT(*) FROM Interactions 
                    WHERE FromUserID = @UserId AND InteractionType = 'LIKE'";
                using (var cmd = new MySqlCommand(likesGivenQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int likesGiven = Convert.ToInt32(cmd.ExecuteScalar());
                    stats.Add("Likes Given", likesGiven.ToString());
                }
                
                // Total de likes recibidos
                string likesReceivedQuery = @"
                    SELECT COUNT(*) FROM Interactions 
                    WHERE ToUserID = @UserId AND InteractionType = 'LIKE'";
                using (var cmd = new MySqlCommand(likesReceivedQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int likesReceived = Convert.ToInt32(cmd.ExecuteScalar());
                    stats.Add("Likes Received", likesReceived.ToString());
                }
                
                // Número de matches
                string matchesQuery = @"
                    SELECT COUNT(*) FROM Matches 
                    WHERE User1ID = @UserId OR User2ID = @UserId";
                using (var cmd = new MySqlCommand(matchesQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int matches = Convert.ToInt32(cmd.ExecuteScalar());
                    stats.Add("Total Matches", matches.ToString());
                }

                // Ratio de éxito (matches / likes recibidos)
                if (stats.ContainsKey("Likes Received") && stats.ContainsKey("Total Matches"))
                {
                    int likesReceived = int.Parse(stats["Likes Received"]);
                    int totalMatches = int.Parse(stats["Total Matches"]);
                    
                    if (likesReceived > 0)
                    {
                        double ratio = (double)totalMatches / likesReceived * 100;
                        stats.Add("Match Success Rate", $"{ratio:F1}%");
                    }
                    else
                    {
                        stats.Add("Match Success Rate", "N/A");
                    }
                }
                
                // Fecha del primer y último match
                string matchDatesQuery = @"
                    SELECT MIN(MatchDate) as FirstMatch, MAX(MatchDate) as LastMatch 
                    FROM Matches 
                    WHERE User1ID = @UserId OR User2ID = @UserId";
                using (var cmd = new MySqlCommand(matchDatesQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("LastMatch")))
                        {
                            DateTime firstMatch = reader.GetDateTime("FirstMatch");
                            DateTime lastMatch = reader.GetDateTime("LastMatch");
                            
                            stats.Add("First Match", firstMatch.ToString("MMM d, yyyy"));
                            stats.Add("Last Match", lastMatch.ToString("MMM d, yyyy"));
                        }
                        else
                        {
                            stats.Add("Match History", "No matches yet");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR getting basic user stats: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }
        
        public Dictionary<string, string> GetUserBehaviorStatistics(int userId)
        {
            var stats = new Dictionary<string, string>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                // Total de perfiles vistos (estimación basada en interacciones)
                string profilesViewedQuery = @"
                    SELECT COUNT(*) FROM Interactions 
                    WHERE FromUserID = @UserId";
                using (var cmd = new MySqlCommand(profilesViewedQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int profilesViewed = Convert.ToInt32(cmd.ExecuteScalar());
                    stats.Add("Profiles Viewed", profilesViewed.ToString());
                }
                
                // Distribución de likes/dislikes
                string distributionQuery = @"
                    SELECT InteractionType, COUNT(*) as Count 
                    FROM Interactions 
                    WHERE FromUserID = @UserId 
                    GROUP BY InteractionType";
                using (var cmd = new MySqlCommand(distributionQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int likes = 0;
                        int dislikes = 0;
                        
                        while (reader.Read())
                        {
                            string type = reader.GetString("InteractionType");
                            int count = reader.GetInt32("Count");
                            
                            if (type == "LIKE") likes = count;
                            else if (type == "DISLIKE") dislikes = count;
                        }
                        
                        int total = likes + dislikes;
                        if (total > 0)
                        {
                            double likePercent = (double)likes / total * 100;
                            stats.Add("Like Ratio", $"{likePercent:F1}% likes, {(100-likePercent):F1}% dislikes");
                        }
                        else
                        {
                            stats.Add("Like Ratio", "No interactions yet");
                        }
                    }
                }
                
                // Carreras/intereses a los que más likes das
                string likedCareersQuery = @"
                    SELECT c.CareerName, COUNT(*) as Count
                    FROM Interactions i
                    JOIN Users u ON i.ToUserID = u.UserID
                    JOIN Careers c ON u.CareerID = c.CareerID
                    WHERE i.FromUserID = @UserId AND i.InteractionType = 'LIKE'
                    GROUP BY c.CareerName
                    ORDER BY Count DESC
                    LIMIT 1";
                using (var cmd = new MySqlCommand(likedCareersQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string careerName = reader.GetString("CareerName");
                            int count = reader.GetInt32("Count");
                            stats.Add("Favorite Career", careerName);
                        }
                        else
                        {
                            stats.Add("Favorite Career", "Not enough data");
                        }
                    }
                }
                
                // Día de la semana con más actividad
                string dayOfWeekQuery = @"
                    SELECT 
                        DAYNAME(InteractionDate) as DayOfWeek,
                        COUNT(*) as Count
                    FROM Interactions
                    WHERE FromUserID = @UserId
                    GROUP BY DayOfWeek
                    ORDER BY Count DESC
                    LIMIT 1";
                using (var cmd = new MySqlCommand(dayOfWeekQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader.GetInt32("Count") > 0)
                        {
                            string dayOfWeek = reader.GetString("DayOfWeek");
                            stats.Add("Most Active Day", dayOfWeek);
                        }
                        else
                        {
                            stats.Add("Most Active Day", "Not enough data");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR getting behavior stats: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }
        
        public Dictionary<string, string> GetUserComparativeStatistics(int userId)
        {
            var stats = new Dictionary<string, string>();
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                    
                // Ranking de likes recibidos
                string rankingQuery = @"
                    SELECT 
                        u.UserID,
                        COUNT(*) as LikeCount,
                        (SELECT COUNT(*) FROM Users) as TotalUsers
                    FROM Interactions i
                    JOIN Users u ON u.UserID = i.ToUserID
                    WHERE i.InteractionType = 'LIKE'
                    GROUP BY u.UserID
                    ORDER BY LikeCount DESC";
                using (var cmd = new MySqlCommand(rankingQuery, _connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int rank = 0;
                        int totalUsers = 0;
                        bool foundUser = false;
                        
                        while (reader.Read())
                        {
                            rank++;
                            totalUsers = reader.GetInt32("TotalUsers");
                            if (reader.GetInt32("UserID") == userId)
                            {
                                foundUser = true;
                                double percentile = 100 - ((double)rank / totalUsers * 100);
                                stats.Add("Popularity Ranking", $"{rank} of {totalUsers} (top {percentile:F1}%)");
                                break;
                            }
                        }
                        
                        if (!foundUser && totalUsers > 0)
                        {
                            stats.Add("Popularity Ranking", $"Below top {totalUsers}");
                        }
                    }
                }
                
                // Comparación con promedio de matches
                string avgMatchesQuery = @"
                    SELECT 
                        (SELECT COUNT(*) FROM Matches WHERE User1ID = @UserId OR User2ID = @UserId) as UserMatches,
                        (SELECT COUNT(*) FROM Matches) / (SELECT COUNT(*) FROM Users) as AvgMatches";
                using (var cmd = new MySqlCommand(avgMatchesQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userMatches = reader.GetInt32("UserMatches");
                            double avgMatches = reader.GetDouble("AvgMatches");
                            
                            double ratio = avgMatches > 0 ? userMatches / avgMatches : 0;
                            string comparison = ratio >= 1 ? $"{ratio:F1}x above average" : $"{(1/ratio):F1}x below average";
                            
                            stats.Add("Matches vs Average", comparison);
                        }
                    }
                }
                
                // Comparación con promedio de likes recibidos
                string avgLikesQuery = @"
                    SELECT 
                        (SELECT COUNT(*) FROM Interactions WHERE ToUserID = @UserId AND InteractionType = 'LIKE') as UserLikes,
                        (SELECT COUNT(*) FROM Interactions WHERE InteractionType = 'LIKE') / (SELECT COUNT(*) FROM Users) as AvgLikes";
                using (var cmd = new MySqlCommand(avgLikesQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userLikes = reader.GetInt32("UserLikes");
                            double avgLikes = reader.GetDouble("AvgLikes");
                            
                            if (avgLikes > 0)
                            {
                                double ratio = userLikes / avgLikes;
                                string comparison = ratio >= 1 ? $"{ratio:F1}x above average" : $"{(1/ratio):F1}x below average";
                                stats.Add("Likes vs Average", comparison);
                            }
                            else
                            {
                                stats.Add("Likes vs Average", "Not enough data");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR getting comparative stats: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            
            return stats;
        }
        
        public Dictionary<string, Dictionary<string, string>> GetAllUserStatistics(int userId)
        {
            var allStats = new Dictionary<string, Dictionary<string, string>>();
            
            allStats["Basic Stats"] = GetUserBasicStatistics(userId);
            allStats["Behavior"] = GetUserBehaviorStatistics(userId);
            allStats["Comparisons"] = GetUserComparativeStatistics(userId);
            
            return allStats;
        }

        // Credit management methods
        public int GetRemainingCredits(int userId)
        {
            ResetCreditsIfNeeded(userId);
            
            string query = @"
                SELECT credits_remaining 
                FROM user_credits 
                WHERE user_id = @UserId";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // First check if the user has a credits record
                string checkQuery = "SELECT COUNT(*) FROM user_credits WHERE user_id = @UserId";
                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                int hasCreditsRecord = Convert.ToInt32(checkCmd.ExecuteScalar());
                
                // If no credits record exists, create one with default values
                if (hasCreditsRecord == 0)
                {
                    string insertQuery = @"
                        INSERT INTO user_credits (user_id, credits_remaining, last_reset_date) 
                        VALUES (@UserId, 10, CURRENT_DATE)";
                    using var insertCmd = new MySqlCommand(insertQuery, _connection);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.ExecuteNonQuery();
                    return 10; // Default credits
                }
                
                // Get the credits remaining
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
        
        public bool UseCredit(int userId)
        {
            string query = @"
                UPDATE user_credits 
                SET credits_remaining = credits_remaining - 1 
                WHERE user_id = @UserId AND credits_remaining > 0";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                int rowsAffected = cmd.ExecuteNonQuery();
                
                return rowsAffected > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
        
        public void ResetCreditsIfNeeded(int userId)
        {
            string query = @"
                UPDATE user_credits 
                SET credits_remaining = 10, last_reset_date = CURRENT_DATE 
                WHERE user_id = @UserId AND DATE(last_reset_date) < DATE(CURRENT_DATE)";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                // First check if the user has a credits record
                string checkQuery = "SELECT COUNT(*) FROM user_credits WHERE user_id = @UserId";
                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                int hasCreditsRecord = Convert.ToInt32(checkCmd.ExecuteScalar());
                
                // If no credits record exists, create one with default values
                if (hasCreditsRecord == 0)
                {
                    string insertQuery = @"
                        INSERT INTO user_credits (user_id, credits_remaining, last_reset_date) 
                        VALUES (@UserId, 10, CURRENT_DATE)";
                    using var insertCmd = new MySqlCommand(insertQuery, _connection);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.ExecuteNonQuery();
                    return; // Record created with default values
                }
                
                // Reset credits if needed
                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        // Methods required by interface
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            
            string query = "SELECT * FROM Users";

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
                        IsVerified = reader.GetBoolean("IsVerified")
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
        
        public User? CreateUser(User user)
        {
            // This method is essentially a renamed RegisterUser
            RegisterUser(user);
            return user;
        }
        
        public bool UpdateUser(User user)
        {
            string query = @"UPDATE Users 
                           SET FullName = @FullName, 
                               Age = @Age, 
                               GenderID = @GenderID, 
                               CareerID = @CareerID, 
                               CityID = @CityID, 
                               OrientationID = @OrientationID, 
                               ProfilePhrase = @ProfilePhrase, 
                               MinPreferredAge = @MinPreferredAge, 
                               MaxPreferredAge = @MaxPreferredAge, 
                               IsVerified = @IsVerified
                           WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Age", user.Age);
                cmd.Parameters.AddWithValue("@GenderID", user.GenderID);
                cmd.Parameters.AddWithValue("@CareerID", user.CareerID);
                cmd.Parameters.AddWithValue("@CityID", user.CityID);
                cmd.Parameters.AddWithValue("@OrientationID", user.OrientationID);
                cmd.Parameters.AddWithValue("@ProfilePhrase", user.ProfilePhrase);
                cmd.Parameters.AddWithValue("@MinPreferredAge", user.MinPreferredAge);
                cmd.Parameters.AddWithValue("@MaxPreferredAge", user.MaxPreferredAge);
                cmd.Parameters.AddWithValue("@IsVerified", user.IsVerified);

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
            string query = "DELETE FROM Users WHERE UserID = @UserID";

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

        // Método para actualizar el perfil enriquecido de un usuario
        public bool UpdateEnrichedProfile(User user)
        {
            string query = @"
                UPDATE Users 
                SET ExtendedDescription = @ExtendedDescription,
                    Hobbies = @Hobbies,
                    FavoriteBooks = @FavoriteBooks,
                    FavoriteMovies = @FavoriteMovies,
                    FavoriteMusic = @FavoriteMusic,
                    InstagramProfile = @InstagramProfile,
                    TwitterProfile = @TwitterProfile,
                    LinkedInProfile = @LinkedInProfile,
                    HasEnrichedProfile = TRUE
                WHERE UserID = @UserID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@ExtendedDescription", user.ExtendedDescription ?? string.Empty);
                cmd.Parameters.AddWithValue("@Hobbies", user.Hobbies ?? string.Empty);
                cmd.Parameters.AddWithValue("@FavoriteBooks", user.FavoriteBooks ?? string.Empty);
                cmd.Parameters.AddWithValue("@FavoriteMovies", user.FavoriteMovies ?? string.Empty);
                cmd.Parameters.AddWithValue("@FavoriteMusic", user.FavoriteMusic ?? string.Empty);
                cmd.Parameters.AddWithValue("@InstagramProfile", user.InstagramProfile ?? string.Empty);
                cmd.Parameters.AddWithValue("@TwitterProfile", user.TwitterProfile ?? string.Empty);
                cmd.Parameters.AddWithValue("@LinkedInProfile", user.LinkedInProfile ?? string.Empty);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating enriched profile: {ex.Message}");
                return false;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
    }
} 