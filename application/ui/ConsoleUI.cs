using System;
using System.Collections.Generic;
using System.Linq;
using campus_love_app.domain.entities;
using Spectre.Console;

namespace campus_love_app.application.ui
{
    public class ConsoleUI
    {
        private readonly string _appName = "Campus Love";
        private readonly string _appVersion = "v1.0";
        private User? _currentUser;
        private UserAccount? _currentAccount;

        public ConsoleUI() { }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }
        
        public void SetCurrentAccount(UserAccount account)
        {
            _currentAccount = account;
        }

        public bool IsUserLoggedIn()
        {
            return _currentUser != null;
        }
        
        public User? GetCurrentUser()
        {
            return _currentUser;
        }

        public void ShowWelcome()
        {
            Console.Clear();
            
            // Create a FigletText banner with the app name
            var title = new FigletText(_appName)
                .Centered()
                .Color(Color.HotPink);
            
            // Create a panel to display the title
            var panel = new Panel(title)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.HotPink)
                .Padding(2, 1);

            // Show the panel in the console
            AnsiConsole.Write(panel);
            
            // Show a stylized rule
            AnsiConsole.Write(new Rule("[italic magenta]Find Love on Your Campus![/]").RuleStyle("magenta").Centered());
            
            AnsiConsole.WriteLine();
            
            // Show a welcome text
            AnsiConsole.MarkupLine("[bold]Welcome to Campus Love![/] The app where you can find [magenta]real connections[/] with other students.");
            AnsiConsole.MarkupLine("Create your profile, share your interests, and [bold magenta]find your match[/].");
            
            AnsiConsole.WriteLine();
            PressAnyKey();
        }

        public int ShowMainMenu()
        {
            Console.Clear();
            // Draw the header
            DrawHeader();

            // Show the current user profile if it exists
            if (_currentUser != null)
            {
                ShowUserProfileSummary(_currentUser);
            }

            var choices = new List<string>();
            
            if (_currentUser == null)
            {
                // User is not logged in
                choices.Add("1. Login");
                choices.Add("2. Register");
                choices.Add("3. Exit");
            }
            else
            {
                // User is logged in
                choices.Add("1. View profiles");
                choices.Add("2. View matches");
                choices.Add("3. View statistics");
                choices.Add("4. Logout");
                choices.Add("5. Exit");
            }

            // Create a selector for the main menu
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[magenta]What would you like to do?[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .AddChoices(choices));

            // Convert the selection to an option number
            return int.Parse(option.Split('.')[0]);
        }

        public (string email, string username, string password, User user) ShowRegistrationScreen()
        {
            Console.Clear();
            DrawHeader("Registration");
            
            var fullName = AnsiConsole.Ask<string>("[magenta]Full Name:[/]");
            var age = AnsiConsole.Ask<int>("[magenta]Age (must be 18+):[/]", 18);
            var email = AnsiConsole.Ask<string>("[magenta]Email:[/]");
            var username = AnsiConsole.Ask<string>("[magenta]Username:[/]");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[magenta]Password:[/]")
                    .PromptStyle("magenta")
                    .Secret()
            );
            var confirmPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("[magenta]Confirm Password:[/]")
                    .PromptStyle("magenta")
                    .Secret()
            );
            
            if (password != confirmPassword)
            {
                throw new Exception("Passwords do not match");
            }
            
            var user = new User
            {
                FullName = fullName,
                Age = age
            };
            
            return (email, username, password, user);
        }

        public (string usernameOrEmail, string password) ShowLoginScreen()
        {
            Console.Clear();
            DrawHeader("Login");
            
            var usernameOrEmail = AnsiConsole.Ask<string>("[magenta]Username or Email:[/]");
            
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[magenta]Password:[/]")
                    .PromptStyle("magenta")
                    .Secret()
            );
            
            return (usernameOrEmail, password);
        }

        public User ShowUserProfileCreationScreen(User baseUser)
        {
            Console.Clear();
            DrawHeader("Complete Your Profile");
            
            AnsiConsole.MarkupLine("[bold]Complete your profile to start finding matches![/]");
            AnsiConsole.WriteLine();
            
            // Here we'll collect the remaining user information
            var profilePhrase = AnsiConsole.Ask<string>("[magenta]Your profile phrase:[/]");
            
            // These would typically be rendered as selection menus with options from the database
            var genderId = AnsiConsole.Ask<int>("[magenta]Gender (1: Male, 2: Female):[/]");
            var orientationId = AnsiConsole.Ask<int>("[magenta]Sexual Orientation (1: Straight, 2: Gay, 3: Bisexual):[/]");
            var careerId = AnsiConsole.Ask<int>("[magenta]Career (ID):[/]");
            var cityId = AnsiConsole.Ask<int>("[magenta]City (ID):[/]");
            
            var minAge = AnsiConsole.Ask<int>("[magenta]Minimum preferred age:[/]", 18);
            var maxAge = AnsiConsole.Ask<int>("[magenta]Maximum preferred age:[/]", 100);
            
            // Apply the values to the base user
            baseUser.ProfilePhrase = profilePhrase;
            baseUser.GenderID = genderId;
            baseUser.OrientationID = orientationId;
            baseUser.CareerID = careerId;
            baseUser.CityID = cityId;
            baseUser.MinPreferredAge = minAge;
            baseUser.MaxPreferredAge = maxAge;
            
            return baseUser;
        }

        public void ShowUserProfile(User user, bool isMatchScreen = false)
        {
            Console.Clear();
            DrawHeader(isMatchScreen ? "Match" : "Profile");
            
            // Create a profile panel
            var profileText = $"[bold]{user.FullName}, {user.Age}[/]";
            var phraseText = $"[grey]Phrase:[/] [italic]{user.ProfilePhrase}[/]";
            
            var profileContent = new Panel(profileText + "\n" + phraseText)
                .Border(BoxBorder.Rounded)
                .BorderColor(isMatchScreen ? Color.Yellow : Color.HotPink)
                .Padding(2, 1)
                .Header(isMatchScreen ? "[yellow]IT'S A MATCH![/]" : $"[magenta]{(user.IsVerified ? "✓ " : "")}{user.FullName}[/]");
            
            AnsiConsole.Write(profileContent);

            AnsiConsole.WriteLine();
            if (!isMatchScreen)
            {
                // If we're on the profiles screen, show like/dislike options
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[magenta]What would you like to do?[/]")
                        .PageSize(3)
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .AddChoices(new[] {
                            "💖 Like",
                            "👎 Dislike",
                            "⬅️ Back to menu"
                        }));
                
                // Here you could return the selected action
            }
            else
            {
                // In the match screen, just show the option to go back
                PressAnyKey();
            }
        }

        public void ShowMatches(List<User> matches)
        {
            Console.Clear();
            DrawHeader("My Matches");
            
            if (matches.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You don't have any matches yet. Keep looking![/]");
                AnsiConsole.WriteLine();
                PressAnyKey();
                return;
            }
            
            AnsiConsole.MarkupLine($"[grey]You have [yellow]{matches.Count}[/] matches so far[/]");
            AnsiConsole.WriteLine();

            // Show matches list
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .AddColumn(new TableColumn("Name").Centered())
                .AddColumn(new TableColumn("Age").Centered())
                .AddColumn(new TableColumn("Action").Centered());

            foreach (var match in matches)
            {
                table.AddRow(
                    $"[bold]{match.FullName}[/]",
                    match.Age.ToString(),
                    $"[link=view:{match.UserID}]View profile[/]"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Interaction to view a specific profile or return to the menu
            var options = new List<string> { "⬅️ Back to menu" };
            options.AddRange(matches.Select(m => $"View profile #{m.UserID}"));
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to do?[/]")
                    .PageSize(Math.Min(10, matches.Count + 1))
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(options));
            
            if (choice != "⬅️ Back to menu")
            {
                // Extract the selected user ID
                string selectedIdText = choice.Replace("View profile #", "");
                if (int.TryParse(selectedIdText, out int selectedId))
                {
                    // Find the corresponding user
                    var selectedUser = matches.First(m => m.UserID == selectedId);
                    // Show their profile
                    ShowUserProfile(selectedUser, true);
                }
            }
        }

        public void ShowStatistics(Dictionary<string, string> stats)
        {
            Console.Clear();
            DrawHeader("Statistics");

            // Create a table for statistics
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("Statistic").LeftAligned())
                .AddColumn(new TableColumn("Value").RightAligned());
            
            foreach (var stat in stats)
            {
                table.AddRow(
                    $"[blue]{stat.Key}[/]",
                    $"[bold white]{stat.Value}[/]"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }

        public void ShowError(string message)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {message}");
            PressAnyKey();
        }

        public void ShowSuccess(string message)
        {
            AnsiConsole.MarkupLine($"[bold green]✓[/] {message}");
            PressAnyKey();
        }
        
        public void ShowLogout()
        {
            AnsiConsole.MarkupLine("[bold green]You have been successfully logged out.[/]");
            _currentUser = null;
            _currentAccount = null;
            PressAnyKey();
        }

        public void ShowGoodbye()
        {
            Console.Clear();
            var title = new FigletText("See you soon!")
                .Centered()
                .Color(Color.HotPink);
            
            AnsiConsole.Write(title);
            AnsiConsole.MarkupLine("[grey]Thanks for using Campus Love. Come back soon![/]");
            
            PressAnyKey();
        }

        // Helper methods

        private void DrawHeader(string title = "")
        {
            // Create a smaller FigletText title for the header
            if (string.IsNullOrEmpty(title))
            {
                var logo = new FigletText(_appName)
                    .Centered()
                    .Color(Color.HotPink);
                
                AnsiConsole.Write(logo);
            }
            else
            {
                AnsiConsole.Write(new Rule($"[bold magenta]{title}[/]").RuleStyle("magenta").Centered());
            }
            
            AnsiConsole.WriteLine();
        }

        private void ShowUserProfileSummary(User user)
        {
            var panel = new Panel($"[bold]Welcome, {user.FullName}![/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Blue)
                .Padding(1, 1);
            
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        private void PressAnyKey()
        {
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
} 