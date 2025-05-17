using System;
using System.Security.Cryptography;
using System.Text;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly MySqlConnection _connection;

        public UserAccountRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public void RegisterAccount(UserAccount account, string plainPassword)
        {
            string query = @"INSERT INTO UserAccounts (UserID, Email, Username, PasswordHash) 
                            VALUES (@UserID, @Email, @Username, @PasswordHash);
                            SELECT LAST_INSERT_ID();";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@UserID", account.UserID);
                cmd.Parameters.AddWithValue("@Email", account.Email);
                cmd.Parameters.AddWithValue("@Username", account.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(plainPassword));

                // Get the inserted ID and assign it to the account
                int accountId = Convert.ToInt32(cmd.ExecuteScalar());
                account.AccountID = accountId;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public UserAccount? GetAccountByUsername(string username)
        {
            return GetAccountBy("Username", username);
        }

        public UserAccount? GetAccountByEmail(string email)
        {
            return GetAccountBy("Email", email);
        }

        public UserAccount? GetAccountByUserId(int userId)
        {
            return GetAccountBy("UserID", userId.ToString());
        }

        private UserAccount? GetAccountBy(string fieldName, string value)
        {
            UserAccount? account = null;
            string query = $"SELECT * FROM UserAccounts WHERE {fieldName} = @Value";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Value", value);
                
                using var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    account = new UserAccount
                    {
                        AccountID = reader.GetInt32("AccountID"),
                        UserID = reader.GetInt32("UserID"),
                        Email = reader.GetString("Email"),
                        Username = reader.GetString("Username"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("LastLoginDate")))
                    {
                        account.LastLoginDate = reader.GetDateTime("LastLoginDate");
                    }
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return account;
        }

        public bool ValidateCredentials(string usernameOrEmail, string password)
        {
            string hashedPassword = HashPassword(password);
            string query = @"SELECT COUNT(*) FROM UserAccounts 
                            WHERE (Username = @Identifier OR Email = @Identifier) 
                            AND PasswordHash = @PasswordHash 
                            AND IsActive = 1";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Identifier", usernameOrEmail);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public void UpdateLastLoginDate(int accountId)
        {
            string query = "UPDATE UserAccounts SET LastLoginDate = NOW() WHERE AccountID = @AccountID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@AccountID", accountId);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public void ChangePassword(int accountId, string newPassword)
        {
            string query = "UPDATE UserAccounts SET PasswordHash = @PasswordHash WHERE AccountID = @AccountID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@AccountID", accountId);
                cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(newPassword));
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public bool UsernameExists(string username)
        {
            return CheckExists("Username", username);
        }

        public bool EmailExists(string email)
        {
            return CheckExists("Email", email);
        }

        private bool CheckExists(string fieldName, string value)
        {
            string query = $"SELECT COUNT(*) FROM UserAccounts WHERE {fieldName} = @Value";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Value", value);
                
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
    }
} 