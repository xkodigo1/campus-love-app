using System;
using campus_love_app.infrastructure.mysql;

namespace campus_love_app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionTest = SingletonConnection.Instance.TestConnection();
            if (!connectionTest)
            {
                ShowError("Could not connect to the database. Exiting the application.");
                return; // Exit if connection fails
            }
        }
        public static void ShowError(string message, Exception ex = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            if (ex != null)
            {
                Console.WriteLine($"  Detail: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner error: {ex.InnerException.Message}");
                }
            }
            Console.ResetColor();
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}