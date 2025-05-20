using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using Spectre.Console;

namespace campus_love_app.application.ui
{
    public class AdminUI
    {
        private readonly string _appName = "Campus Love Admin";
        private Administrator _currentAdmin;
        private readonly IAdminRepository _adminRepository;

        public AdminUI(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public void SetCurrentAdmin(Administrator admin)
        {
            _currentAdmin = admin;
        }

        public bool IsAdminLoggedIn()
        {
            return _currentAdmin != null;
        }

        public void ShowWelcome()
        {
            Console.Clear();
            
            var title = new FigletText(_appName)
                .Centered()
                .Color(Color.Red);
            
            var panel = new Panel(title)
                .Border(BoxBorder.Double)
                .BorderColor(Color.Red)
                .Padding(2, 1);

            AnsiConsole.Write(panel);
            
            AnsiConsole.Write(new Rule("[italic red]Administrative Panel[/]").RuleStyle("red").Centered());
            
            AnsiConsole.WriteLine();
            
            AnsiConsole.MarkupLine("[bold]Welcome to the Campus Love Admin Panel[/]");
            AnsiConsole.MarkupLine("[red]This interface provides advanced administrative tools. Authorized personnel only.[/]");
            
            AnsiConsole.WriteLine();
            PressAnyKey();
        }

        public int ShowMainMenu()
        {
            Console.Clear();
            DrawHeader();

            if (_currentAdmin != null)
            {
                AnsiConsole.MarkupLine($"[bold red]Logged in as: {_currentAdmin.Username} (Admin)[/]");
                AnsiConsole.WriteLine();
            }

            var choices = new List<string>();
            
            if (!IsAdminLoggedIn())
            {
                // Admin is not logged in
                choices.Add("1. Login as Admin");
                choices.Add("2. Exit");
            }
            else
            {
                // Admin is logged in
                choices.Add("1. User Management");
                choices.Add("2. App Statistics");
                choices.Add("3. System Settings");
                choices.Add("4. Logout");
                choices.Add("5. Exit");
            }

            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]What would you like to do?[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(choices));

            return int.Parse(option.Split('.')[0]);
        }

        public (string username, string password) ShowAdminLoginScreen()
        {
            Console.Clear();
            DrawHeader("Admin Login");
            
            AnsiConsole.MarkupLine("[bold red]Administrative Access Only[/]");
            AnsiConsole.WriteLine();
            
            var username = AnsiConsole.Ask<string>("[red]Username:[/]");
            
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[red]Password:[/]")
                    .PromptStyle("red")
                    .Secret()
            );
            
            return (username, password);
        }

        public void ShowUserManagementMenu()
        {
            bool exitMenu = false;
            
            while (!exitMenu && IsAdminLoggedIn())
            {
                Console.Clear();
                DrawHeader("User Management");
                
                var choices = new List<string>
                {
                    "1. List All Users",
                    "2. Search Users",
                    "3. View User Details",
                    "4. Verify User",
                    "5. Ban/Unban User",
                    "6. Delete User",
                    "7. Back to Main Menu"
                };
                
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[red]Select an option:[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(foreground: Color.Red))
                        .AddChoices(choices));
                
                int selectedOption = int.Parse(option.Split('.')[0]);
                
                switch (selectedOption)
                {
                    case 1: // List All Users
                        ShowAllUsers();
                        break;
                        
                    case 2: // Search Users
                        ShowUserSearch();
                        break;
                        
                    case 3: // View User Details
                        ShowUserDetailsPrompt();
                        break;
                        
                    case 4: // Verify User
                        VerifyUserPrompt();
                        break;
                        
                    case 5: // Ban/Unban User
                        BanUnbanUserPrompt();
                        break;
                        
                    case 6: // Delete User
                        DeleteUserPrompt();
                        break;
                        
                    case 7: // Back to Main Menu
                        exitMenu = true;
                        break;
                }
            }
        }

        private void ShowAllUsers()
        {
            Console.Clear();
            DrawHeader("All Users");
            
            var users = _adminRepository.GetAllUsers();
            
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No users found in the system.[/]");
                PressAnyKey();
                return;
            }
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .AddColumn(new TableColumn("ID").Centered())
                .AddColumn(new TableColumn("Name").LeftAligned())
                .AddColumn(new TableColumn("Age").Centered())
                .AddColumn(new TableColumn("Verified").Centered())
                .AddColumn(new TableColumn("Actions").Centered());
                
            foreach (var user in users)
            {
                table.AddRow(
                    user.UserID.ToString(),
                    $"[bold]{user.FullName}[/]",
                    user.Age.ToString(),
                    user.IsVerified ? "[green]Yes[/]" : "[grey]No[/]",
                    $"[link=view:{user.UserID}]View[/] | [link=edit:{user.UserID}]Edit[/] | [link=delete:{user.UserID}]Delete[/]"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Pagination controls if there are many users
            if (users.Count > 20)
            {
                AnsiConsole.MarkupLine("[grey]Showing first 20 users. Use search to find specific users.[/]");
            }
            
            PressAnyKey();
        }

        private void ShowUserSearch()
        {
            Console.Clear();
            DrawHeader("Search Users");
            
            var searchTerm = AnsiConsole.Ask<string>("[red]Enter search term (name, email, username):[/]");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                AnsiConsole.MarkupLine("[yellow]Search term cannot be empty.[/]");
                PressAnyKey();
                return;
            }
            
            var users = _adminRepository.SearchUsers(searchTerm);
            
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]No users found matching '{searchTerm}'.[/]");
                PressAnyKey();
                return;
            }
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .AddColumn(new TableColumn("ID").Centered())
                .AddColumn(new TableColumn("Name").LeftAligned())
                .AddColumn(new TableColumn("Age").Centered())
                .AddColumn(new TableColumn("Verified").Centered())
                .AddColumn(new TableColumn("Actions").Centered());
                
            foreach (var user in users)
            {
                table.AddRow(
                    user.UserID.ToString(),
                    $"[bold]{user.FullName}[/]",
                    user.Age.ToString(),
                    user.IsVerified ? "[green]Yes[/]" : "[grey]No[/]",
                    $"[link=view:{user.UserID}]View[/] | [link=edit:{user.UserID}]Edit[/] | [link=delete:{user.UserID}]Delete[/]"
                );
            }
            
            AnsiConsole.MarkupLine($"[grey]Found {users.Count} users matching '{searchTerm}':[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }

        private void ShowUserDetailsPrompt()
        {
            Console.Clear();
            DrawHeader("View User Details");
            
            var userId = AnsiConsole.Ask<int>("[red]Enter User ID:[/]");
            ShowUserDetails(userId);
        }

        private void ShowUserDetails(int userId)
        {
            Console.Clear();
            DrawHeader("User Details");
            
            var user = _adminRepository.GetUserById(userId);
            
            if (user == null)
            {
                AnsiConsole.MarkupLine($"[yellow]User with ID {userId} not found.[/]");
                PressAnyKey();
                return;
            }
            
            // Basic user info
            var profilePanel = new Panel($"[bold]{user.FullName}, {user.Age}[/] (ID: {user.UserID})")
                .Border(BoxBorder.Rounded)
                .BorderColor(user.IsVerified ? Color.Green : Color.Grey)
                .Header($"{(user.IsVerified ? "[green]Verified[/]" : "[grey]Not Verified[/]")}")
                .Padding(1, 1);
            
            AnsiConsole.Write(profilePanel);
            AnsiConsole.WriteLine();
            
            // Details table
            var detailsTable = new Table()
                .Border(TableBorder.Simple)
                .BorderColor(Color.Red)
                .AddColumn(new TableColumn("Field").LeftAligned())
                .AddColumn(new TableColumn("Value").LeftAligned())
                .Expand();
                
            detailsTable.AddRow("Profile Phrase", user.ProfilePhrase);
            detailsTable.AddRow("Gender ID", user.GenderID.ToString());
            detailsTable.AddRow("Career ID", user.CareerID.ToString());
            detailsTable.AddRow("City ID", user.CityID.ToString());
            detailsTable.AddRow("Sexual Orientation ID", user.OrientationID.ToString());
            detailsTable.AddRow("Preferred Age Range", $"{user.MinPreferredAge} - {user.MaxPreferredAge}");
            
            AnsiConsole.Write(detailsTable);
            AnsiConsole.WriteLine();
            
            // Enriched profile data if available
            if (user.HasEnrichedProfile)
            {
                var enrichedTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Magenta1)
                    .Title("[bold]Enriched Profile Data[/]")
                    .AddColumn(new TableColumn("Field").LeftAligned())
                    .AddColumn(new TableColumn("Value").LeftAligned())
                    .Expand();
                    
                if (!string.IsNullOrEmpty(user.ExtendedDescription))
                    enrichedTable.AddRow("Description", user.ExtendedDescription);
                
                if (!string.IsNullOrEmpty(user.Hobbies))
                    enrichedTable.AddRow("Hobbies", user.Hobbies);
                
                if (!string.IsNullOrEmpty(user.FavoriteBooks))
                    enrichedTable.AddRow("Favorite Books", user.FavoriteBooks);
                
                if (!string.IsNullOrEmpty(user.FavoriteMovies))
                    enrichedTable.AddRow("Favorite Movies", user.FavoriteMovies);
                
                if (!string.IsNullOrEmpty(user.FavoriteMusic))
                    enrichedTable.AddRow("Favorite Music", user.FavoriteMusic);
                
                // Only show this section if there's actually enriched data
                if (enrichedTable.Rows.Count > 0)
                {
                    AnsiConsole.Write(enrichedTable);
                    AnsiConsole.WriteLine();
                }
            }
            
            // Actions
            var options = new List<string>
            {
                user.IsVerified ? "Unverify User" : "Verify User",
                "Ban User",
                "Delete User",
                "Back"
            };
            
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Select an action:[/]")
                    .PageSize(5)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(options));
            
            switch (option)
            {
                case "Verify User":
                    if (_adminRepository.VerifyUser(userId))
                    {
                        AnsiConsole.MarkupLine("[green]User has been verified successfully.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Failed to verify user.[/]");
                    }
                    break;
                    
                case "Unverify User":
                    // This would need to be implemented in the repository
                    AnsiConsole.MarkupLine("[yellow]Unverify functionality not yet implemented.[/]");
                    break;
                    
                case "Ban User":
                    if (_adminRepository.BanUser(userId))
                    {
                        AnsiConsole.MarkupLine("[green]User has been banned successfully.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Failed to ban user.[/]");
                    }
                    break;
                    
                case "Delete User":
                    if (AnsiConsole.Confirm($"[red]Are you sure you want to delete user {user.FullName}?[/]", false))
                    {
                        if (_adminRepository.DeleteUser(userId))
                        {
                            AnsiConsole.MarkupLine("[green]User has been deleted successfully.[/]");
                            PressAnyKey();
                            return; // Return since the user no longer exists
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[yellow]Failed to delete user.[/]");
                        }
                    }
                    break;
            }
            
            PressAnyKey();
        }

        private void VerifyUserPrompt()
        {
            Console.Clear();
            DrawHeader("Verify User");
            
            var userId = AnsiConsole.Ask<int>("[red]Enter User ID to verify:[/]");
            
            if (_adminRepository.VerifyUser(userId))
            {
                AnsiConsole.MarkupLine($"[green]User {userId} has been verified successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Failed to verify user {userId}. User might not exist.[/]");
            }
            
            PressAnyKey();
        }

        private void BanUnbanUserPrompt()
        {
            Console.Clear();
            DrawHeader("Ban/Unban User");
            
            var userId = AnsiConsole.Ask<int>("[red]Enter User ID:[/]");
            
            var options = new[] { "Ban User", "Unban User", "Cancel" };
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Select action:[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(options));
            
            switch (option)
            {
                case "Ban User":
                    if (_adminRepository.BanUser(userId))
                    {
                        AnsiConsole.MarkupLine($"[green]User {userId} has been banned successfully.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]Failed to ban user {userId}. User might not exist.[/]");
                    }
                    break;
                    
                case "Unban User":
                    if (_adminRepository.UnbanUser(userId))
                    {
                        AnsiConsole.MarkupLine($"[green]User {userId} has been unbanned successfully.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]Failed to unban user {userId}. User might not exist.[/]");
                    }
                    break;
            }
            
            PressAnyKey();
        }

        private void DeleteUserPrompt()
        {
            Console.Clear();
            DrawHeader("Delete User");
            
            AnsiConsole.MarkupLine("[bold red]WARNING: This action cannot be undone.[/]");
            AnsiConsole.WriteLine();
            
            var userId = AnsiConsole.Ask<int>("[red]Enter User ID to delete:[/]");
            
            if (AnsiConsole.Confirm($"[red]Are you sure you want to delete user {userId}?[/]", false))
            {
                if (_adminRepository.DeleteUser(userId))
                {
                    AnsiConsole.MarkupLine($"[green]User {userId} has been deleted successfully.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Failed to delete user {userId}. User might not exist.[/]");
                }
            }
            
            PressAnyKey();
        }

        private void DrawHeader(string title = "")
        {
            if (string.IsNullOrEmpty(title))
            {
                var logo = new FigletText(_appName)
                    .Centered()
                    .Color(Color.Red);
                
                AnsiConsole.Write(logo);
            }
            else
            {
                AnsiConsole.Write(new Rule($"[bold red]{title}[/]").RuleStyle("red").Centered());
            }
            
            AnsiConsole.WriteLine();
        }

        private void PressAnyKey()
        {
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        public void ShowError(string message)
        {
            Console.Clear();
            DrawHeader("Error");

            var panel = new Panel($"[bold red]{message}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red)
                .Padding(2, 1);

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }

        public void ShowSystemSettings()
        {
            Console.Clear();
            DrawHeader("System Settings");
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]System Configuration[/]")
                .AddColumn(new TableColumn("Setting").Width(30).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(20).LeftAligned())
                .Expand();
                
            table.AddRow("App Name", "Campus Love");
            table.AddRow("Version", "1.0.0");
            table.AddRow("Max Daily Credits", "10");
            table.AddRow("Credit Refresh Time", "00:00 UTC");
            table.AddRow("Max Profiles Per Day", "50");
            table.AddRow("Password Min Length", "8");
            table.AddRow("Account Lockout Duration", "30 minutes");
            table.AddRow("Session Timeout", "60 minutes");
            table.AddRow("Backup Frequency", "Daily");
            table.AddRow("Matching Algorithm Version", "1.2");
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            AnsiConsole.MarkupLine("[yellow]System settings configuration module is under development.[/]");
            AnsiConsole.MarkupLine("[yellow]More options will be available in future updates.[/]");
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }

        public void ShowAppStatistics()
        {
            bool exitMenu = false;
            
            while (!exitMenu && IsAdminLoggedIn())
            {
                Console.Clear();
                DrawHeader("App Statistics");
                
                var choices = new List<string>
                {
                    "1. User Statistics",
                    "2. User Demographics",
                    "3. Interaction Statistics",
                    "4. Communication Statistics",
                    "5. Usage Statistics",
                    "6. Credit Statistics",
                    "7. Moderation Statistics",
                    "8. All Statistics Dashboard",
                    "9. Back to Main Menu"
                };
                
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[red]Select statistics type:[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(foreground: Color.Red))
                        .AddChoices(choices));
                
                int selectedOption = int.Parse(option.Split('.')[0]);
                
                switch (selectedOption)
                {
                    case 1: // User Statistics
                        ShowUserStatistics();
                        break;
                        
                    case 2: // User Demographics
                        ShowUserDemographics();
                        break;
                        
                    case 3: // Interaction Statistics
                        ShowInteractionStatistics();
                        break;
                        
                    case 4: // Communication Statistics
                        ShowCommunicationStatistics();
                        break;
                        
                    case 5: // Usage Statistics
                        ShowUsageStatistics();
                        break;
                        
                    case 6: // Credit Statistics
                        ShowCreditStatistics();
                        break;
                        
                    case 7: // Moderation Statistics
                        ShowModerationStatistics();
                        break;
                        
                    case 8: // All Statistics Dashboard
                        ShowStatisticsDashboard();
                        break;
                        
                    case 9: // Back to Main Menu
                        exitMenu = true;
                        break;
                }
            }
        }
        
        private void ShowStatisticsDashboard()
        {
            Console.Clear();
            DrawHeader("Statistics Dashboard");
            
            AnsiConsole.MarkupLine("[bold red]All Key Metrics Overview[/]");
            AnsiConsole.WriteLine();
            
            // Get all statistics
            var userStats = _adminRepository.GetUserStatistics();
            var interactionStats = _adminRepository.GetInteractionStatistics();
            var communicationStats = _adminRepository.GetCommunicationStatistics();
            var usageStats = _adminRepository.GetUsageStatistics();
            var creditStats = _adminRepository.GetCreditStatistics();
            var moderationStats = _adminRepository.GetModerationStatistics();
            
            // Create a dashboard layout with 3 columns
            var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left").SplitRows(
                        new Layout("Users"),
                        new Layout("Interactions")
                    ),
                    new Layout("Middle").SplitRows(
                        new Layout("Communications"),
                        new Layout("Activity")
                    ),
                    new Layout("Right").SplitRows(
                        new Layout("Credits"),
                        new Layout("Moderation")
                    )
                );
            
            // Users panel
            var usersPanel = new Panel(new Rows(
                    new Text("Users", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Total Users: [green]{userStats["TotalUsers"]}[/]"),
                    new Markup($"New (7d): [green]{userStats["NewUsersLast7Days"]}[/]"),
                    new Markup($"Verified: [green]{userStats["VerifiedUsers"]}[/] ({userStats.GetValueOrDefault("VerifiedPercentage", 0)}%)"),
                    new Markup($"Enriched Profiles: [green]{userStats["EnrichedProfiles"]}[/]")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Users"].Update(usersPanel);
            
            // Interactions panel
            var interactionsPanel = new Panel(new Rows(
                    new Text("Interactions", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Total Likes: [green]{interactionStats.GetValueOrDefault("TotalLikes", 0)}[/]"),
                    new Markup($"Total Matches: [green]{interactionStats.GetValueOrDefault("TotalMatches", 0)}[/]"),
                    new Markup($"Match Rate: [green]{interactionStats.GetValueOrDefault("MatchRate", 0)}%[/]"),
                    new Markup($"Daily Likes Avg: [green]{interactionStats.GetValueOrDefault("DailyLikesAverage", 0)}[/]")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Interactions"].Update(interactionsPanel);
            
            // Communications panel
            var commsPanel = new Panel(new Rows(
                    new Text("Communications", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Active Conversations: [green]{communicationStats.GetValueOrDefault("ActiveConversations", 0)}[/]"),
                    new Markup($"Total Messages: [green]{communicationStats.GetValueOrDefault("TotalMessages", 0)}[/]"),
                    new Markup($"Unread Messages: [green]{communicationStats.GetValueOrDefault("UnreadMessages", 0)}[/]"),
                    new Markup($"Long Conversations: [green]{communicationStats.GetValueOrDefault("LongConversations", 0)}[/]")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Communications"].Update(commsPanel);
            
            // Activity panel
            var activityPanel = new Panel(new Rows(
                    new Text("Activity", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Active Users (24h): [green]{usageStats.GetValueOrDefault("ActiveUsers24h", 0)}[/]"),
                    new Markup($"Active Users (7d): [green]{usageStats.GetValueOrDefault("ActiveUsers7d", 0)}[/]"),
                    new Markup($"Most Active Hour: [green]{usageStats.GetValueOrDefault("MostActiveHour", 0)}:00[/]"),
                    new Markup($"Most Active Day: [green]{GetDayName(usageStats.GetValueOrDefault("MostActiveDay", 1))}[/]")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Activity"].Update(activityPanel);
            
            // Credits panel
            var creditsPanel = new Panel(new Rows(
                    new Text("Credits", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Credits Available: [green]{creditStats.GetValueOrDefault("TotalCreditsAvailable", 0)}[/]"),
                    new Markup($"Used Today: [green]{creditStats.GetValueOrDefault("CreditsUsedToday", 0)}[/]"),
                    new Markup($"Avg. Per User: [green]{creditStats.GetValueOrDefault("AverageCreditsPerUser", 0)}[/]"),
                    new Markup($"Zero Credits: [green]{creditStats.GetValueOrDefault("UsersWithNoCredits", 0)}[/]")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Credits"].Update(creditsPanel);
            
            // Moderation panel
            var moderationPanel = new Panel(new Rows(
                    new Text("Moderation", new Style(Color.Red, decoration: Decoration.Bold)),
                    new Markup($"Reported Users: [green]{moderationStats.GetValueOrDefault("TotalReportedUsers", 0)}[/]"),
                    new Markup($"Pending Reports: [green]{moderationStats.GetValueOrDefault("PendingReports", 0)}[/]"),
                    new Markup($"Banned Users: [green]{moderationStats.GetValueOrDefault("BannedUsers", 0)}[/]"),
                    new Markup($"Resolution Time: [green]{moderationStats.GetValueOrDefault("AverageReportResolutionTime", 0)}[/] hrs")
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            layout["Moderation"].Update(moderationPanel);
            
            // Render the layout
            AnsiConsole.Write(layout);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowUserStatistics()
        {
            Console.Clear();
            DrawHeader("User Statistics");
            
            var stats = _adminRepository.GetUserStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]User Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(25).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(10).RightAligned());
            
            table.AddRow("Total Users", stats["TotalUsers"].ToString());
            table.AddRow("New Users (Last 7 days)", stats["NewUsersLast7Days"].ToString());
            table.AddRow("New Users (Last 30 days)", stats["NewUsersLast30Days"].ToString());
            table.AddRow("Verified Users", $"{stats["VerifiedUsers"]} ({stats.GetValueOrDefault("VerifiedPercentage", 0)}%)");
            table.AddRow("Users with Enriched Profiles", $"{stats["EnrichedProfiles"]} ({stats.GetValueOrDefault("EnrichedProfilePercentage", 0)}%)");
            table.AddRow("Active Users", $"{stats["ActiveUsers"]} ({stats.GetValueOrDefault("ActivePercentage", 0)}%)");
            
            AnsiConsole.Write(table);
            
            // Create a bar chart for user growth
            var growthChart = new BarChart()
                .Width(60)
                .Label("[bold red]User Growth[/]")
                .CenterLabel()
                .AddItem("Last 7 days", stats["NewUsersLast7Days"], Color.Green)
                .AddItem("Last 30 days", stats["NewUsersLast30Days"], Color.Yellow);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(growthChart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowUserDemographics()
        {
            Console.Clear();
            DrawHeader("User Demographics");
            
            var demographics = _adminRepository.GetUserDemographics();
            
            // Gender distribution
            var genderTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Gender Distribution[/]")
                .AddColumn(new TableColumn("Gender").Width(20).LeftAligned())
                .AddColumn(new TableColumn("Count").Width(10).RightAligned())
                .AddColumn(new TableColumn("%").Width(10).RightAligned());
            
            int totalUsers = demographics["Gender"].Values.Sum();
            foreach (var kvp in demographics["Gender"])
            {
                int percentage = totalUsers > 0 ? (int)((double)kvp.Value / totalUsers * 100) : 0;
                genderTable.AddRow(kvp.Key, kvp.Value.ToString(), $"{percentage}%");
            }
            
            // Orientation distribution
            var orientationTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Sexual Orientation Distribution[/]")
                .AddColumn(new TableColumn("Orientation").Width(20).LeftAligned())
                .AddColumn(new TableColumn("Count").Width(10).RightAligned())
                .AddColumn(new TableColumn("%").Width(10).RightAligned());
            
            totalUsers = demographics["Orientation"].Values.Sum();
            foreach (var kvp in demographics["Orientation"])
            {
                int percentage = totalUsers > 0 ? (int)((double)kvp.Value / totalUsers * 100) : 0;
                orientationTable.AddRow(kvp.Key, kvp.Value.ToString(), $"{percentage}%");
            }
            
            // Age distribution
            var ageTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Age Distribution[/]")
                .AddColumn(new TableColumn("Age Group").Width(20).LeftAligned())
                .AddColumn(new TableColumn("Count").Width(10).RightAligned())
                .AddColumn(new TableColumn("%").Width(10).RightAligned());
            
            totalUsers = demographics["AgeGroup"].Values.Sum();
            foreach (var kvp in demographics["AgeGroup"].OrderBy(x => x.Key))
            {
                int percentage = totalUsers > 0 ? (int)((double)kvp.Value / totalUsers * 100) : 0;
                ageTable.AddRow(kvp.Key, kvp.Value.ToString(), $"{percentage}%");
            }
            
            // Create layout
            var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left").SplitRows(
                        new Layout("Gender"),
                        new Layout("Orientation")
                    ),
                    new Layout("Right").SplitRows(
                        new Layout("Age"),
                        new Layout("Career")
                    )
                );
            
            layout["Gender"].Update(genderTable);
            layout["Orientation"].Update(orientationTable);
            layout["Age"].Update(ageTable);
            
            // Career table (might be too large for layout)
            var careerTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Top 5 Careers[/]")
                .AddColumn(new TableColumn("Career").Width(20).LeftAligned())
                .AddColumn(new TableColumn("Count").Width(10).RightAligned())
                .AddColumn(new TableColumn("%").Width(10).RightAligned());
            
            totalUsers = demographics["Career"].Values.Sum();
            foreach (var kvp in demographics["Career"].OrderByDescending(x => x.Value).Take(5))
            {
                int percentage = totalUsers > 0 ? (int)((double)kvp.Value / totalUsers * 100) : 0;
                careerTable.AddRow(kvp.Key, kvp.Value.ToString(), $"{percentage}%");
            }
            
            layout["Career"].Update(careerTable);
            
            AnsiConsole.Write(layout);
            
            AnsiConsole.WriteLine();
            
            // Location table
            var locationTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Top 5 Locations[/]")
                .AddColumn(new TableColumn("City").Width(20).LeftAligned())
                .AddColumn(new TableColumn("Count").Width(10).RightAligned())
                .AddColumn(new TableColumn("%").Width(10).RightAligned());
            
            totalUsers = demographics["Location"].Values.Sum();
            foreach (var kvp in demographics["Location"])
            {
                int percentage = totalUsers > 0 ? (int)((double)kvp.Value / totalUsers * 100) : 0;
                locationTable.AddRow(kvp.Key, kvp.Value.ToString(), $"{percentage}%");
            }
            
            AnsiConsole.Write(locationTable);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowInteractionStatistics()
        {
            Console.Clear();
            DrawHeader("Interaction Statistics");
            
            var stats = _adminRepository.GetInteractionStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Interaction Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(25).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(15).RightAligned());
            
            table.AddRow("Total Likes", stats.GetValueOrDefault("TotalLikes", 0).ToString());
            table.AddRow("Total Dislikes", stats.GetValueOrDefault("TotalDislikes", 0).ToString());
            table.AddRow("Total Matches", stats.GetValueOrDefault("TotalMatches", 0).ToString());
            table.AddRow("Daily Likes Average", stats.GetValueOrDefault("DailyLikesAverage", 0).ToString());
            table.AddRow("Match Rate", $"{stats.GetValueOrDefault("MatchRate", 0)}%");
            
            if (stats.ContainsKey("MostPopularUserID"))
            {
                var user = _adminRepository.GetUserById(stats["MostPopularUserID"]);
                if (user != null)
                {
                    table.AddRow("Most Popular User", $"{user.FullName} (ID: {user.UserID})");
                    table.AddRow("Likes Received", stats.GetValueOrDefault("MostPopularUserLikes", 0).ToString());
                }
            }
            
            AnsiConsole.Write(table);
            
            // Create a bar chart for interactions
            var chart = new BarChart()
                .Width(60)
                .Label("[bold red]Interactions[/]")
                .CenterLabel()
                .AddItem("Likes", stats.GetValueOrDefault("TotalLikes", 0), Color.Green)
                .AddItem("Dislikes", stats.GetValueOrDefault("TotalDislikes", 0), Color.Red)
                .AddItem("Matches", stats.GetValueOrDefault("TotalMatches", 0), Color.Yellow);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowCommunicationStatistics()
        {
            Console.Clear();
            DrawHeader("Communication Statistics");
            
            var stats = _adminRepository.GetCommunicationStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Communication Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(30).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(15).RightAligned());
            
            table.AddRow("Total Conversations", stats.GetValueOrDefault("TotalConversations", 0).ToString());
            table.AddRow("Total Messages", stats.GetValueOrDefault("TotalMessages", 0).ToString());
            table.AddRow("Average Messages Per Conversation", stats.GetValueOrDefault("AverageMessagesPerConversation", 0).ToString());
            table.AddRow("Active Conversations (Last 7 Days)", stats.GetValueOrDefault("ActiveConversations", 0).ToString());
            table.AddRow("Unread Messages", stats.GetValueOrDefault("UnreadMessages", 0).ToString());
            table.AddRow("Long Conversations (>10 messages)", stats.GetValueOrDefault("LongConversations", 0).ToString());
            
            AnsiConsole.Write(table);
            
            // Create a pie chart for messages status
            var chart = new BarChart()
                .Width(60)
                .Label("[bold red]Messages Status[/]")
                .CenterLabel()
                .AddItem("Total Messages", stats.GetValueOrDefault("TotalMessages", 0), Color.Green)
                .AddItem("Unread Messages", stats.GetValueOrDefault("UnreadMessages", 0), Color.Red);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowUsageStatistics()
        {
            Console.Clear();
            DrawHeader("Usage Statistics");
            
            var stats = _adminRepository.GetUsageStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Usage Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(30).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(15).RightAligned());
            
            string mostActiveHour = stats.ContainsKey("MostActiveHour") ? 
                $"{stats["MostActiveHour"]}:00 - {stats["MostActiveHour"]+1}:00" : "N/A";
                
            string mostActiveDay = stats.ContainsKey("MostActiveDay") ?
                GetDayName(stats["MostActiveDay"]) : "N/A";
                
            table.AddRow("Most Active Hour", mostActiveHour);
            table.AddRow("Most Active Day", mostActiveDay);
            table.AddRow("Active Users (Last 24h)", stats.GetValueOrDefault("ActiveUsers24h", 0).ToString());
            table.AddRow("Active Users (Last 7d)", stats.GetValueOrDefault("ActiveUsers7d", 0).ToString());
            table.AddRow("Active Users (Last 30d)", stats.GetValueOrDefault("ActiveUsers30d", 0).ToString());
            
            AnsiConsole.Write(table);
            
            // Create a bar chart for active users
            var chart = new BarChart()
                .Width(60)
                .Label("[bold red]Active Users[/]")
                .CenterLabel()
                .AddItem("Last 24 Hours", stats.GetValueOrDefault("ActiveUsers24h", 0), Color.Green)
                .AddItem("Last 7 Days", stats.GetValueOrDefault("ActiveUsers7d", 0), Color.Yellow)
                .AddItem("Last 30 Days", stats.GetValueOrDefault("ActiveUsers30d", 0), Color.Red);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowCreditStatistics()
        {
            Console.Clear();
            DrawHeader("Credit Statistics");
            
            var stats = _adminRepository.GetCreditStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Credit Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(30).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(15).RightAligned());
            
            table.AddRow("Total Credits Available", stats.GetValueOrDefault("TotalCreditsAvailable", 0).ToString());
            table.AddRow("Users with Max Credits", stats.GetValueOrDefault("UsersWithMaxCredits", 0).ToString());
            table.AddRow("Users with No Credits", stats.GetValueOrDefault("UsersWithNoCredits", 0).ToString());
            table.AddRow("Average Credits Per User", stats.GetValueOrDefault("AverageCreditsPerUser", 0).ToString());
            table.AddRow("Credits Used Today", stats.GetValueOrDefault("CreditsUsedToday", 0).ToString());
            table.AddRow("Average Credits Used Per Day", stats.GetValueOrDefault("AverageCreditsPerDay", 0).ToString());
            
            AnsiConsole.Write(table);
            
            // Create a bar chart for credit distribution
            var chart = new BarChart()
                .Width(60)
                .Label("[bold red]Credit Status[/]")
                .CenterLabel()
                .AddItem("Max Credits", stats.GetValueOrDefault("UsersWithMaxCredits", 0), Color.Green)
                .AddItem("No Credits", stats.GetValueOrDefault("UsersWithNoCredits", 0), Color.Red);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private void ShowModerationStatistics()
        {
            Console.Clear();
            DrawHeader("Moderation Statistics");
            
            var stats = _adminRepository.GetModerationStatistics();
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]Moderation Statistics[/]")
                .AddColumn(new TableColumn("Metric").Width(30).LeftAligned())
                .AddColumn(new TableColumn("Value").Width(15).RightAligned());
            
            table.AddRow("Total Reported Users", stats.GetValueOrDefault("TotalReportedUsers", 0).ToString());
            table.AddRow("Unverified Users", stats.GetValueOrDefault("UnverifiedUsers", 0).ToString());
            table.AddRow("Banned Users", stats.GetValueOrDefault("BannedUsers", 0).ToString());
            table.AddRow("Pending Reports", stats.GetValueOrDefault("PendingReports", 0).ToString());
            table.AddRow("Reports Processed (Last 7 Days)", stats.GetValueOrDefault("ReportsProcessedLast7Days", 0).ToString());
            table.AddRow("Avg. Report Resolution Time", $"{stats.GetValueOrDefault("AverageReportResolutionTime", 0)} hours");
            
            AnsiConsole.Write(table);
            
            // Create a bar chart for moderation activities
            var chart = new BarChart()
                .Width(60)
                .Label("[bold red]Moderation Status[/]")
                .CenterLabel()
                .AddItem("Total Reports", stats.GetValueOrDefault("TotalReportedUsers", 0), Color.Yellow)
                .AddItem("Pending", stats.GetValueOrDefault("PendingReports", 0), Color.Red)
                .AddItem("Processed", stats.GetValueOrDefault("ReportsProcessedLast7Days", 0), Color.Green);
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Note: These are currently demonstration values as the moderation system is being developed.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Data updated as of: [/][green]{0}[/]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AnsiConsole.WriteLine();
            
            PressAnyKey();
        }
        
        private string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Sunday",
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                _ => "Unknown"
            };
        }
    }
} 