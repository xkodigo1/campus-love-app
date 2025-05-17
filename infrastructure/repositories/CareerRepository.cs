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

        public List<Career> GetAll()
        {
            var careers = new List<Career>();
            string query = "SELECT CareerID, Name FROM Careers";

            using var cmd = new MySqlCommand(query, _connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                careers.Add(new Career
                {
                    CareerID = reader.GetInt32("CareerID"),
                    Name = reader.GetString("Name")
                });
            }

            return careers;
        }
    }
}
