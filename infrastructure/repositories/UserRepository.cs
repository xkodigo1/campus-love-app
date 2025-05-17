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

        public void LikeUser(int fromUserId, int toUserId)
        {
            try
            {
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
                        IsVerified = reader.GetBoolean("IsVerified")
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
    }
} 