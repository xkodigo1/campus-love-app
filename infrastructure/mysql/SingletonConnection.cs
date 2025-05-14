using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace campus_love_app.infrastructure.mysql
{
   public class SingletonConnection
    {
        private static SingletonConnection _instance;
        private MySqlConnection _connection;

        private SingletonConnection()
        {
            string connectionString = "server=localhost;port=3306;database=campus-love-db;user=root;password=kodigo777;";
            
            _connection = new MySqlConnection(connectionString);
        }

        public static SingletonConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SingletonConnection();
                }
                return _instance;
            }
        }

        public MySqlConnection GetConnection()
        {
            return _connection;
        }

        public bool TestConnection()
        {
            try
            {
                _connection.Open();
                Console.WriteLine("✅ Connection to the database was successful.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection to the database failed: {ex.Message}");
                return false;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
    }
}