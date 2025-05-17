using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class InterestRepository : IInterestRepository
    {
        private readonly MySqlConnection _connection;

        public InterestRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        public List<Interest> GetAll()
        {
            var interests = new List<Interest>();
            string query = "SELECT InterestID, Name FROM Interests";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
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