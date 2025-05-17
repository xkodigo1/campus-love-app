using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class CareerRepository : ICareerRepository
    {
        private readonly MySqlConnection _connection;

        public CareerRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        public List<Career> GetAllCareers()
        {
            var careers = new List<Career>();
            
            string query = "SELECT * FROM Careers ORDER BY CareerName";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var career = new Career
                    {
                        CareerID = reader.GetInt32("CareerID"),
                        CareerName = reader.GetString("CareerName")
                    };
                    
                    careers.Add(career);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return careers;
        }

        public int GetOrCreateCareer(string careerName)
        {
            if (string.IsNullOrWhiteSpace(careerName))
                throw new ArgumentException("Career name cannot be empty");
            
            // First check if the career exists
            string checkQuery = "SELECT CareerID FROM Careers WHERE CareerName = @CareerName";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@CareerName", careerName);
                
                var existingId = checkCmd.ExecuteScalar();
                
                if (existingId != null && existingId != DBNull.Value)
                {
                    return Convert.ToInt32(existingId);
                }
                
                // If we get here, the career doesn't exist and needs to be created
                string insertQuery = "INSERT INTO Careers (CareerName) VALUES (@CareerName); SELECT LAST_INSERT_ID();";
                
                using var insertCmd = new MySqlCommand(insertQuery, _connection);
                insertCmd.Parameters.AddWithValue("@CareerName", careerName);
                
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
    }
}
