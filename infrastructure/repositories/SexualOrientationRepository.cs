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
        }

        public List<SexualOrientation> GetAll()
        {
            var orientations = new List<SexualOrientation>();
            string query = "SELECT SexualOrientationID, Name FROM SexualOrientations";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orientations.Add(new SexualOrientation
                    {
                        SexualOrientationID = reader.GetInt32("SexualOrientationID"),
                        Name = reader.GetString("Name")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return orientations;
        }
    }
} 