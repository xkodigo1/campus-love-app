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
            string query = @"INSERT INTO Users (Name, Surname, BirthDate, Email, Password, GenderID, CareerID, CityID, SexualOrientationID, Bio, IsVerified) 
                            VALUES (@Name, @Surname, @BirthDate, @Email, @Password, @GenderID, @CareerID, @CityID, @SexualOrientationID, @Bio, @IsVerified);
                            SELECT LAST_INSERT_ID();";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Surname", user.Surname);
                cmd.Parameters.AddWithValue("@BirthDate", user.BirthDate);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@GenderID", user.GenderID);
                cmd.Parameters.AddWithValue("@CareerID", user.CareerID);
                cmd.Parameters.AddWithValue("@CityID", user.CityID);
                cmd.Parameters.AddWithValue("@SexualOrientationID", user.SexualOrientationID);
                cmd.Parameters.AddWithValue("@Bio", user.Bio);
                cmd.Parameters.AddWithValue("@IsVerified", user.IsVerified);

                // Get the inserted ID and assign it to the user
                int userId = Convert.ToInt32(cmd.ExecuteScalar());
                user.UserID = userId;
                
                // If the user has interests, save them in the UserInterests table
                if (user.Interests != null && user.Interests.Count > 0)
                {
                    foreach (var interest in user.Interests)
                    {
                        string interestQuery = "INSERT INTO UserInterests (UserID, InterestID) VALUES (@UserID, @InterestID)";
                        using var interestCmd = new MySqlCommand(interestQuery, _connection);
                        interestCmd.Parameters.AddWithValue("@UserID", userId);
                        interestCmd.Parameters.AddWithValue("@InterestID", interest.InterestID);
                        interestCmd.ExecuteNonQuery();
                    }
                }
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
                SELECT u.*, g.Name AS GenderName, c.Name AS CareerName, city.Name AS CityName, 
                       so.Name AS SexualOrientationName, r.Name AS RegionName, country.Name AS CountryName
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN Cities city ON u.CityID = city.CityID
                JOIN Regions r ON city.RegionID = r.RegionID
                JOIN Countries country ON r.CountryID = country.CountryID
                JOIN SexualOrientations so ON u.SexualOrientationID = so.SexualOrientationID
                WHERE u.UserID <> @UserId
                AND u.UserID NOT IN (
                    SELECT ToUserID FROM Interactions WHERE FromUserID = @UserId
                )";

            // Add filtering based on sexual orientation
            if (currentUser.SexualOrientationID == 1) // Straight
            {
                if (currentUser.GenderID == 1) // Male looking for Female
                    query += " AND u.GenderID = 2";
                else // Female looking for Male
                    query += " AND u.GenderID = 1";
            }
            else if (currentUser.SexualOrientationID == 2) // Gay
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
                        Name = reader.GetString("Name"),
                        Surname = reader.GetString("Surname"),
                        BirthDate = reader.GetDateTime("BirthDate"),
                        Email = reader.GetString("Email"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        SexualOrientationID = reader.GetInt32("SexualOrientationID"),
                        Bio = reader.GetString("Bio"),
                        IsVerified = reader.GetBoolean("IsVerified"),
                        // You might want to add additional properties from the joined tables
                    };
                    
                    users.Add(user);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            // Load interests for each user
            foreach (var user in users)
            {
                user.Interests = GetUserInterests(user.UserID);
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
                string interactionQuery = "INSERT INTO Interactions (FromUserID, ToUserID, Type, InteractionDate) VALUES (@FromUserID, @ToUserID, 'LIKE', NOW())";
                using var interactionCmd = new MySqlCommand(interactionQuery, _connection);
                interactionCmd.Parameters.AddWithValue("@FromUserID", fromUserId);
                interactionCmd.Parameters.AddWithValue("@ToUserID", toUserId);
                interactionCmd.ExecuteNonQuery();

                // Check if there's a match (if the other person has already liked this user)
                string matchCheckQuery = "SELECT COUNT(*) FROM Interactions WHERE FromUserID = @ToUserID AND ToUserID = @FromUserID AND Type = 'LIKE'";
                using var matchCheckCmd = new MySqlCommand(matchCheckQuery, _connection);
                matchCheckCmd.Parameters.AddWithValue("@FromUserID", fromUserId);
                matchCheckCmd.Parameters.AddWithValue("@ToUserID", toUserId);
                
                int hasMatch = Convert.ToInt32(matchCheckCmd.ExecuteScalar());
                
                if (hasMatch > 0)
                {
                    // It's a match! Create entry in Matches table
                    string createMatchQuery = "INSERT INTO Matches (User1ID, User2ID, MatchDate) VALUES (@User1ID, @User2ID, NOW())";
                    using var createMatchCmd = new MySqlCommand(createMatchQuery, _connection);
                    createMatchCmd.Parameters.AddWithValue("@User1ID", fromUserId);
                    createMatchCmd.Parameters.AddWithValue("@User2ID", toUserId);
                    createMatchCmd.ExecuteNonQuery();
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
                SELECT u.*, g.Name AS GenderName, c.Name AS CareerName, city.Name AS CityName, 
                       so.Name AS SexualOrientationName
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN Cities city ON u.CityID = city.CityID
                JOIN SexualOrientations so ON u.SexualOrientationID = so.SexualOrientationID
                WHERE u.UserID IN (
                    SELECT User2ID FROM Matches WHERE User1ID = @UserId
                    UNION
                    SELECT User1ID FROM Matches WHERE User2ID = @UserId
                )";

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
                        Name = reader.GetString("Name"),
                        Surname = reader.GetString("Surname"),
                        BirthDate = reader.GetDateTime("BirthDate"),
                        Email = reader.GetString("Email"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        SexualOrientationID = reader.GetInt32("SexualOrientationID"),
                        Bio = reader.GetString("Bio"),
                        IsVerified = reader.GetBoolean("IsVerified")
                    };
                    
                    matches.Add(user);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            // Load interests for each matched user
            foreach (var user in matches)
            {
                user.Interests = GetUserInterests(user.UserID);
            }

            return matches;
        }

        // Helper method to get a user by ID
        private User GetUserById(int userId)
        {
            User user = null;
            
            string query = @"
                SELECT u.*, g.Name AS GenderName, c.Name AS CareerName, city.Name AS CityName, 
                       so.Name AS SexualOrientationName
                FROM Users u
                JOIN Genders g ON u.GenderID = g.GenderID
                JOIN Careers c ON u.CareerID = c.CareerID
                JOIN Cities city ON u.CityID = city.CityID
                JOIN SexualOrientations so ON u.SexualOrientationID = so.SexualOrientationID
                WHERE u.UserID = @UserId";

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
                        Name = reader.GetString("Name"),
                        Surname = reader.GetString("Surname"),
                        BirthDate = reader.GetDateTime("BirthDate"),
                        Email = reader.GetString("Email"),
                        GenderID = reader.GetInt32("GenderID"),
                        CareerID = reader.GetInt32("CareerID"),
                        CityID = reader.GetInt32("CityID"),
                        SexualOrientationID = reader.GetInt32("SexualOrientationID"),
                        Bio = reader.GetString("Bio"),
                        IsVerified = reader.GetBoolean("IsVerified")
                    };
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            if (user != null)
            {
                user.Interests = GetUserInterests(userId);
            }

            return user;
        }

        // Helper method to get user interests
        private List<Interest> GetUserInterests(int userId)
        {
            var interests = new List<Interest>();
            
            string query = @"
                SELECT i.* 
                FROM Interests i
                JOIN UserInterests ui ON i.InterestID = ui.InterestID
                WHERE ui.UserID = @UserId";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    interests.Add(new Interest
                    {
                        InterestID = reader.GetInt32("InterestID"),
                        Name = reader.GetString("Name")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return interests;
        }
    }
} 