using System;
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
            
            string query = "SELECT * FROM Countries ORDER BY CountryName";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var country = new Country
                    {
                        CountryID = reader.GetInt32("CountryID"),
                        CountryName = reader.GetString("CountryName")
                    };
                    
                    countries.Add(country);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return countries;
        }

        public List<Region> GetRegionsByCountryId(int countryId)
        {
            var regions = new List<Region>();
            
            string query = "SELECT * FROM Regions WHERE CountryID = @CountryID ORDER BY RegionName";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var region = new Region
                    {
                        RegionID = reader.GetInt32("RegionID"),
                        RegionName = reader.GetString("RegionName"),
                        CountryID = reader.GetInt32("CountryID")
                    };
                    
                    regions.Add(region);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return regions;
        }

        public List<City> GetCitiesByRegionId(int regionId)
        {
            var cities = new List<City>();
            
            string query = "SELECT * FROM Cities WHERE RegionID = @RegionID ORDER BY CityName";

            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@RegionID", regionId);
                
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var city = new City
                    {
                        CityID = reader.GetInt32("CityID"),
                        CityName = reader.GetString("CityName"),
                        RegionID = reader.GetInt32("RegionID")
                    };
                    
                    cities.Add(city);
                }
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return cities;
        }

        public int GetOrCreateCountry(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                throw new ArgumentException("Country name cannot be empty");
            
            // First check if the country exists
            string checkQuery = "SELECT CountryID FROM Countries WHERE CountryName = @CountryName";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@CountryName", countryName);
                
                var existingId = checkCmd.ExecuteScalar();
                
                if (existingId != null && existingId != DBNull.Value)
                {
                    return Convert.ToInt32(existingId);
                }
                
                // If we get here, the country doesn't exist and needs to be created
                string insertQuery = "INSERT INTO Countries (CountryName) VALUES (@CountryName); SELECT LAST_INSERT_ID();";
                
                using var insertCmd = new MySqlCommand(insertQuery, _connection);
                insertCmd.Parameters.AddWithValue("@CountryName", countryName);
                
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public int GetOrCreateRegion(string regionName, int countryId)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be empty");
            
            // First check if the region exists
            string checkQuery = "SELECT RegionID FROM Regions WHERE RegionName = @RegionName AND CountryID = @CountryID";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@RegionName", regionName);
                checkCmd.Parameters.AddWithValue("@CountryID", countryId);
                
                var existingId = checkCmd.ExecuteScalar();
                
                if (existingId != null && existingId != DBNull.Value)
                {
                    return Convert.ToInt32(existingId);
                }
                
                // If we get here, the region doesn't exist and needs to be created
                string insertQuery = "INSERT INTO Regions (RegionName, CountryID) VALUES (@RegionName, @CountryID); SELECT LAST_INSERT_ID();";
                
                using var insertCmd = new MySqlCommand(insertQuery, _connection);
                insertCmd.Parameters.AddWithValue("@RegionName", regionName);
                insertCmd.Parameters.AddWithValue("@CountryID", countryId);
                
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public int GetOrCreateCity(string cityName, int regionId)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name cannot be empty");
            
            // First check if the city exists
            string checkQuery = "SELECT CityID FROM Cities WHERE CityName = @CityName AND RegionID = @RegionID";
            
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                using var checkCmd = new MySqlCommand(checkQuery, _connection);
                checkCmd.Parameters.AddWithValue("@CityName", cityName);
                checkCmd.Parameters.AddWithValue("@RegionID", regionId);
                
                var existingId = checkCmd.ExecuteScalar();
                
                if (existingId != null && existingId != DBNull.Value)
                {
                    return Convert.ToInt32(existingId);
                }
                
                // If we get here, the city doesn't exist and needs to be created
                string insertQuery = "INSERT INTO Cities (CityName, RegionID) VALUES (@CityName, @RegionID); SELECT LAST_INSERT_ID();";
                
                using var insertCmd = new MySqlCommand(insertQuery, _connection);
                insertCmd.Parameters.AddWithValue("@CityName", cityName);
                insertCmd.Parameters.AddWithValue("@RegionID", regionId);
                
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