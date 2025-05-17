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
        }

        public List<Gender> GetAll()
        {
            var genders = new List<Gender>();
            string query = "SELECT GenderID, Name FROM Genders";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    genders.Add(new Gender
                    {
                        GenderID = reader.GetInt32("GenderID"),
                        Name = reader.GetString("Name")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return genders;
        }
    }
} 