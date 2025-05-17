using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using campus_love_app.infrastructure.mysql;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly MySqlConnection _connection;

        public LocationRepository()
        {
            _connection = SingletonConnection.Instance.GetConnection();
        }

        public List<Country> GetAllCountries()
        {
            var countries = new List<Country>();
            string query = "SELECT CountryID, Name FROM Countries";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    countries.Add(new Country
                    {
                        CountryID = reader.GetInt32("CountryID"),
                        Name = reader.GetString("Name")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return countries;
        }

        public List<Region> GetRegionsByCountry(int countryId)
        {
            var regions = new List<Region>();
            string query = "SELECT RegionID, Name, CountryID FROM Regions WHERE CountryID = @CountryID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    regions.Add(new Region
                    {
                        RegionID = reader.GetInt32("RegionID"),
                        Name = reader.GetString("Name"),
                        CountryID = reader.GetInt32("CountryID")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return regions;
        }

        public List<City> GetCitiesByRegion(int regionId)
        {
            var cities = new List<City>();
            string query = "SELECT CityID, Name, RegionID FROM Cities WHERE RegionID = @RegionID";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@RegionID", regionId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cities.Add(new City
                    {
                        CityID = reader.GetInt32("CityID"),
                        Name = reader.GetString("Name"),
                        RegionID = reader.GetInt32("RegionID")
                    });
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return cities;
        }
    }
} 