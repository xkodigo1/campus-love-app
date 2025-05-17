using System;
using System.Collections.Generic;
using campus_love_app.application.ui;
using campus_love_app.domain.entities;
using campus_love_app.infrastructure.mysql;
using Spectre.Console;

namespace campus_love_app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Test the database connection
            var connectionTest = SingletonConnection.Instance.TestConnection();
            if (!connectionTest)
            {
                ShowError("Could not connect to the database. Exiting the application.");
                return; // Exit if connection fails
            }

            // Initialize the UI
            var ui = new ConsoleUI();

            // Show the welcome screen
            ui.ShowWelcome();

            // Demo: Sample user
            var demoUser = new User
            {
                UserID = 1,
                FullName = "John Smith",
                Age = 22,
                GenderID = 1,
                OrientationID = 1,
                CareerID = 1,
                ProfilePhrase = "I love programming and video games",
                IsVerified = true,
                CityID = 1,
                MinPreferredAge = 18,
                MaxPreferredAge = 28
            };

            // Set the current user for UI
            ui.SetCurrentUser(demoUser);

            // Main application loop (DEMO)
            bool exit = false;
            while (!exit)
            {
                try
                {
                    int option = ui.ShowMainMenu();

                    switch (option)
                    {
                        case 1: // Register
                            ui.ShowSuccess("Registration feature not implemented in this demo!");
                            break;

                        case 2: // View profiles
                            // Demo: Simulate profile to view
                            var demoProfile = new User
                            {
                                UserID = 2,
                                FullName = "Anna Garcia",
                                Age = 21,
                                GenderID = 2,
                                OrientationID = 1,
                                CareerID = 2,
                                ProfilePhrase = "I like art, music and nature",
                                IsVerified = false
                            };
                            ui.ShowUserProfile(demoProfile);
                            break;

                        case 3: // View matches
                            // Demo: Create list of matches
                            var demoMatches = new List<User>
                            {
                                new User { UserID = 3, FullName = "Maria Lopez", Age = 20 },
                                new User { UserID = 4, FullName = "Carlos Ruiz", Age = 23 },
                                new User { UserID = 5, FullName = "Sofia Torres", Age = 19 }
                            };
                            ui.ShowMatches(demoMatches);
                            break;

                        case 4: // Statistics
                            // Demo: Example statistics
                            var stats = new Dictionary<string, string>
                            {
                                { "Total users", "256" },
                                { "Matches made", "78" },
                                { "Most liked user", "John Smith (32 likes)" },
                                { "Most popular interest", "Music (45%)" },
                                { "Most common career", "Computer Science (23%)" },
                                { "Match ratio", "18%" }
                            };
                            ui.ShowStatistics(stats);
                            break;

                        case 5: // Exit
                            exit = true;
                            ui.ShowGoodbye();
                            break;

                        default:
                            ui.ShowError("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"An unexpected error occurred: {ex.Message}", ex);
                }
            }
        }

        // Method to show errors
        public static void ShowError(string message, Exception? ex = null)
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