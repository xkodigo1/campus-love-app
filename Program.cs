using System;
using System.Collections.Generic;
using campus_love_app.application.ui;
using campus_love_app.domain.entities;
using campus_love_app.infrastructure.mysql;
using campus_love_app.infrastructure.repositories;
using campus_love_app.application.services;
using campus_love_app.domain.ports;
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

            // Initialize repositories
            UserRepository userRepository = new UserRepository();
            IUserAccountRepository accountRepository = new UserAccountRepository();
            ILocationRepository locationRepository = new LocationRepository();
            ICareerRepository careerRepository = new CareerRepository();
            IGenderRepository genderRepository = new GenderRepository();
            ISexualOrientationRepository orientationRepository = new SexualOrientationRepository();
            IChatRepository chatRepository = new ChatRepository();
            IAdminRepository adminRepository = new AdminRepository();
            
            // Initialize services
            var loginService = new LoginService(userRepository, accountRepository);

            // First show admin or user selection
            var accessType = ShowAccessTypeSelection();

            if (accessType == "admin")
            {
                // Admin path
                RunAdminInterface(adminRepository);
            }
            else
            {
                // User path
                RunUserInterface(userRepository, accountRepository, locationRepository, careerRepository, genderRepository, orientationRepository, chatRepository, loginService);
            }
        }

        private static string ShowAccessTypeSelection()
        {
            Console.Clear();

            var title = new FigletText("Campus Love")
                .Centered()
                .Color(Color.HotPink);
            
            var panel = new Panel(title)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.HotPink)
                .Padding(2, 1);

            AnsiConsole.Write(panel);
            
            AnsiConsole.Write(new Rule("[italic magenta]Choose Access Type[/]").RuleStyle("magenta").Centered());
            
            AnsiConsole.WriteLine();

            // Present the choice between admin and user access
            var choices = new List<string>
            {
                "Administrator Access",
                "User Access"
            };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[magenta]Select your access type:[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .AddChoices(choices));

            return choice == "Administrator Access" ? "admin" : "user";
        }

        private static void RunAdminInterface(IAdminRepository adminRepository)
        {
            // Initialize the Admin UI
            var adminUI = new AdminUI(adminRepository);

            // Show the welcome screen
            adminUI.ShowWelcome();

            // Main application loop
            bool exit = false;
            while (!exit)
            {
                try
                {
                    int option = adminUI.ShowMainMenu();

                    if (!adminUI.IsAdminLoggedIn())
                    {
                        // Admin is not logged in
                        switch (option)
                        {
                            case 1: // Admin Login
                                HandleAdminLogin(adminUI, adminRepository);
                                break;

                            case 2: // Switch to User Access
                                return; // Return to let the program restart with user access

                            case 3: // Exit
                                exit = true;
                                Console.Clear();
                                AnsiConsole.MarkupLine("[grey]Thanks for using Campus Love Admin Panel.[/]");
                                Console.WriteLine();
                                break;

                            default:
                                adminUI.ShowError("Invalid option. Please try again.");
                                break;
                        }
                    }
                    else
                    {
                        // Admin is logged in
                        switch (option)
                        {
                            case 1: // User Management
                                adminUI.ShowUserManagementMenu();
                                break;

                            case 2: // App Statistics
                                adminUI.ShowError("Statistics feature not yet implemented.");
                                break;

                            case 3: // System Settings
                                adminUI.ShowError("System settings not yet implemented.");
                                break;

                            case 4: // Logout
                                adminUI.SetCurrentAdmin(null);
                                AnsiConsole.MarkupLine("[green]You have been successfully logged out.[/]");
                                break;

                            case 5: // Exit
                                exit = true;
                                Console.Clear();
                                AnsiConsole.MarkupLine("[grey]Thanks for using Campus Love Admin Panel.[/]");
                                Console.WriteLine();
                                break;

                            default:
                                adminUI.ShowError("Invalid option. Please try again.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"An unexpected error occurred: {ex.Message}", ex);
                }
            }
        }

        private static void HandleAdminLogin(AdminUI adminUI, IAdminRepository adminRepository)
        {
            try
            {
                var (username, password) = adminUI.ShowAdminLoginScreen();
                
                if (adminRepository.ValidateAdmin(username, password))
                {
                    var admin = adminRepository.GetAdminByUsername(username);
                    adminUI.SetCurrentAdmin(admin);
                    AnsiConsole.MarkupLine($"[green]Welcome, {admin.FullName}![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid admin credentials.[/]");
                }
                
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Login failed: {ex.Message}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
        }

        private static void RunUserInterface(
            UserRepository userRepository, 
            IUserAccountRepository accountRepository,
            ILocationRepository locationRepository,
            ICareerRepository careerRepository,
            IGenderRepository genderRepository,
            ISexualOrientationRepository orientationRepository,
            IChatRepository chatRepository,
            LoginService loginService)
        {
            // Initialize the UI
            var ui = new ConsoleUI(locationRepository, careerRepository, genderRepository, orientationRepository, userRepository);

            // Show the welcome screen
            ui.ShowWelcome();

            // Main application loop
            bool exit = false;
            while (!exit)
            {
                try
                {
                    int option = ui.ShowMainMenu();

                    if (!ui.IsUserLoggedIn())
                    {
                        // User is not logged in
                        switch (option)
                        {
                            case 1: // Login
                                HandleLogin(ui, loginService);
                                break;

                            case 2: // Register
                                HandleRegistration(ui, loginService);
                                break;

                            case 3: // Exit
                                exit = true;
                                ui.ShowGoodbye();
                                break;

                            default:
                                ui.ShowError("Invalid option. Please try again.");
                                break;
                        }
                    }
                    else
                    {
                        // User is logged in
                        switch (option)
                        {
                            case 1: // View profiles
                                User? currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    var availableProfiles = userRepository.GetAvailableProfiles(currentUser.UserID);
                                    if (availableProfiles.Count > 0)
                                    {
                                        ui.ShowUserProfile(availableProfiles[0], false, availableProfiles, 0);
                                    }
                                    else
                                    {
                                        ui.ShowError("No available profiles found.");
                                    }
                                }
                                break;

                            case 2: // View matches
                                currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    var matches = userRepository.GetMatches(currentUser.UserID);
                                    ui.ShowMatches(matches, chatRepository);
                                }
                                break;

                            case 3: // Messages/Conversations
                                currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    var conversations = chatRepository.GetUserConversations(currentUser.UserID);
                                    ui.ShowConversations(conversations, currentUser.UserID, chatRepository);
                                }
                                break;

                            case 4: // Statistics
                                currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    var userStats = userRepository.GetAllUserStatistics(currentUser.UserID);
                                    ui.ShowUserDetailedStatistics(userStats);
                                }
                                else
                                {
                                    ui.ShowError("You must be logged in to view your statistics.");
                                }
                                break;
                                
                            case 5: // View daily credits
                                currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    ui.ShowRemainingCredits();
                                }
                                break;

                            case 6: // Enrich profile
                                currentUser = ui.GetCurrentUser();
                                if (currentUser != null)
                                {
                                    ui.ShowEnrichedProfileEditor(currentUser);
                                }
                                break;
                                
                            case 7: // Logout
                                ui.ShowLogout();
                                break;

                            case 8: // Exit
                                exit = true;
                                ui.ShowGoodbye();
                                break;

                            default:
                                ui.ShowError("Invalid option. Please try again.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"An unexpected error occurred: {ex.Message}", ex);
                }
            }
        }

        private static void HandleLogin(ConsoleUI ui, LoginService loginService)
        {
            try
            {
                var (usernameOrEmail, password) = ui.ShowLoginScreen();
                var (user, account) = loginService.Login(usernameOrEmail, password);
                
                if (user != null && account != null)
                {
                    ui.SetCurrentUser(user);
                    ui.SetCurrentAccount(account);
                    ui.ShowSuccess($"Welcome back, {user.FullName}!");
                }
                else
                {
                    ui.ShowError("Invalid username/email or password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ui.ShowError($"Login failed: {ex.Message}");
            }
        }

        private static void HandleRegistration(ConsoleUI ui, LoginService loginService)
        {
            try
            {
                // Step 1: Collect basic user information and account details
                var (email, username, password, baseUser) = ui.ShowRegistrationScreen();
                
                // Step 2: Collect profile details
                var fullUser = ui.ShowUserProfileCreationScreen(baseUser);
                
                // Step 3: Register the user in the system
                var (user, account) = loginService.Register(email, username, password, fullUser);
                
                // Step 4: Log the user in
                ui.SetCurrentUser(user);
                ui.SetCurrentAccount(account);
                
                ui.ShowSuccess("Registration successful! Welcome to Campus Love!");
            }
            catch (Exception ex)
            {
                ui.ShowError($"Registration failed: {ex.Message}");
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