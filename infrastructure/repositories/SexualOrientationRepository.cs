using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class SexualOrientationRepository : ISexualOrientationRepository
    {
        private readonly MySqlConnection _connection;

        public SexualOrientationRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
            EnsureDefaultOrientationsExist();
        }

        public List<SexualOrientation> GetAllOrientations()
        {
            var orientations = new List<SexualOrientation>();
            
            string query = "SELECT * FROM SexualOrientations ORDER BY OrientationID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var orientation = new SexualOrientation
                    {
                        OrientationID = reader.GetInt32("OrientationID"),
                        OrientationName = reader.GetString("OrientationName")
                    };
                    
                    orientations.Add(orientation);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return orientations;
        }

        // Make sure default orientations exist in the database
        private void EnsureDefaultOrientationsExist()
        {
            string[] defaultOrientations = { "Straight", "Gay", "Bisexual", "Other" };
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();
                
                // Check if any orientations exist
                string countQuery = "SELECT COUNT(*) FROM SexualOrientations";
                using var countCmd = new MySqlCommand(countQuery, _connection);
                var count = Convert.ToInt32(countCmd.ExecuteScalar());
                
                // If no orientations exist, add the defaults
                if (count == 0)
                {
                    foreach (var orientation in defaultOrientations)
                    {
                        string insertQuery = "INSERT INTO SexualOrientations (OrientationName) VALUES (@OrientationName)";
                        using var insertCmd = new MySqlCommand(insertQuery, _connection);
                        insertCmd.Parameters.AddWithValue("@OrientationName", orientation);
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