using System;
using System.Collections.Generic;
using System.Linq;
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
                choices.Add("2. User Access (Regular Login)");
                choices.Add("3. Exit");
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
    }
} 