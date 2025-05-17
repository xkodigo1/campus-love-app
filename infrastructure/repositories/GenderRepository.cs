using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class GenderRepository : IGenderRepository
    {
        private readonly MySqlConnection _connection;

        public GenderRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
            EnsureDefaultGendersExist();
        }

        public List<Gender> GetAllGenders()
        {
            var genders = new List<Gender>();
            
            string query = "SELECT * FROM Genders ORDER BY GenderID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var gender = new Gender
                    {
                        GenderID = reader.GetInt32("GenderID"),
                        GenderName = reader.GetString("GenderName")
                    };
                    
                    genders.Add(gender);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return genders;
        }

        // Make sure default genders exist in the database
        private void EnsureDefaultGendersExist()
        {
            string[] defaultGenders = { "Male", "Female", "Other" };
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                
                // Check if any genders exist
                string countQuery = "SELECT COUNT(*) FROM Genders";
                using var countCmd = new MySqlCommand(countQuery, _connection);
                var count = Convert.ToInt32(countCmd.ExecuteScalar());
                
                // If no genders exist, add the defaults
                if (count == 0)
                {
                    foreach (var gender in defaultGenders)
                    {
                        string insertQuery = "INSERT INTO Genders (GenderName) VALUES (@GenderName)";
                        using var insertCmd = new MySqlCommand(insertQuery, _connection);
                        insertCmd.Parameters.AddWithValue("@GenderName", gender);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
    }
} 