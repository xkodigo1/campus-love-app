using System;
using System.Collections.Generic;
using System.Linq;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using Spectre.Console;

namespace campus_love_app.application.ui
{
    public class ConsoleUI
    {
        private readonly string _appName = "Campus Love";
        private readonly string _appVersion = "v1.0";
        private User? _currentUser;
        private UserAccount? _currentAccount;
        private readonly ILocationRepository? _locationRepository;
        private readonly ICareerRepository? _careerRepository;
        private readonly IGenderRepository? _genderRepository;
        private readonly ISexualOrientationRepository? _orientationRepository;
        private readonly IUserRepository? _userRepository;

        public ConsoleUI() { }

        public ConsoleUI(ILocationRepository locationRepository, ICareerRepository careerRepository,
                        IGenderRepository genderRepository, ISexualOrientationRepository orientationRepository)
        {
            _locationRepository = locationRepository;
            _careerRepository = careerRepository;
            _genderRepository = genderRepository;
            _orientationRepository = orientationRepository;
        }

        public ConsoleUI(ILocationRepository locationRepository, ICareerRepository careerRepository,
                        IGenderRepository genderRepository, ISexualOrientationRepository orientationRepository,
                        IUserRepository userRepository) : this(locationRepository, careerRepository, genderRepository, orientationRepository)
        {
            _userRepository = userRepository;
        }

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
                choices.Add("3. Messages");
                choices.Add("4. View statistics");
                choices.Add("5. View daily credits");
                choices.Add("6. Enrich my profile");
                choices.Add("7. Logout");
                choices.Add("8. Exit");
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
            
            // Gender selection
            int genderId = 1;
            if (_genderRepository != null)
            {
                var genders = _genderRepository.GetAllGenders();
                if (genders.Count > 0)
                {
                    var genderSelection = AnsiConsole.Prompt(
                        new SelectionPrompt<Gender>()
                            .Title("[magenta]Select your gender:[/]")
                            .PageSize(Math.Max(3, Math.Min(10, genders.Count)))
                            .HighlightStyle(new Style(foreground: Color.HotPink))
                            .UseConverter(g => g.GenderName)
                            .AddChoices(genders));
                    
                    genderId = genderSelection.GenderID;
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: No genders found in database. Using default value.[/]");
                }
            }
            else
            {
                var genderId_str = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[magenta]Select your gender:[/]")
                        .PageSize(3)
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .AddChoices(new[] {
                            "1. Male",
                            "2. Female",
                            "3. Other"
                        }));
                
                genderId = int.Parse(genderId_str.Split('.')[0]);
            }
            
            // Sexual orientation selection
            int orientationId = 1;
            if (_orientationRepository != null)
            {
                var orientations = _orientationRepository.GetAllOrientations();
                if (orientations.Count > 0)
                {
                    var orientationSelection = AnsiConsole.Prompt(
                        new SelectionPrompt<SexualOrientation>()
                            .Title("[magenta]Select your sexual orientation:[/]")
                            .PageSize(Math.Max(3, Math.Min(10, orientations.Count)))
                            .HighlightStyle(new Style(foreground: Color.HotPink))
                            .UseConverter(o => o.OrientationName)
                            .AddChoices(orientations));
                    
                    orientationId = orientationSelection.OrientationID;
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: No orientations found in database. Using default value.[/]");
                }
            }
            else
            {
                var orientationId_str = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[magenta]Select your sexual orientation:[/]")
                        .PageSize(4)
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .AddChoices(new[] {
                            "1. Straight",
                            "2. Gay",
                            "3. Bisexual",
                            "4. Other"
                        }));
                
                orientationId = int.Parse(orientationId_str.Split('.')[0]);
            }
            
            // Career selection
            int careerId = 1;
            if (_careerRepository != null)
            {
                try 
                {
                    // Get existing careers
                    var careers = _careerRepository.GetAllCareers();
                    
                    // Add option to enter a new career
                    string enterNewCareerOption = "Enter new career...";
                    
                    // Display careers if any exist
                    List<string> careerOptions = new List<string>();
                    if (careers.Count > 0)
                    {
                        careerOptions = careers.Select(c => c.CareerName).ToList();
                        careerOptions.Add(enterNewCareerOption);
                        
                        var careerChoice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[magenta]Select your career:[/]")
                                .PageSize(Math.Max(3, Math.Min(10, careerOptions.Count)))
                                .HighlightStyle(new Style(foreground: Color.HotPink))
                                .AddChoices(careerOptions));
                        
                        if (careerChoice == enterNewCareerOption)
                        {
                            // User wants to enter a new career
                            var careerName = AnsiConsole.Ask<string>("[magenta]Enter your career:[/]");
                            careerId = _careerRepository.GetOrCreateCareer(careerName);
                        }
                        else
                        {
                            // User selected an existing career
                            var selectedCareer = careers.FirstOrDefault(c => c.CareerName == careerChoice);
                            if (selectedCareer != null)
                            {
                                careerId = selectedCareer.CareerID;
                            }
                        }
                    }
                    else
                    {
                        // No careers exist yet, just ask for input
                        var careerName = AnsiConsole.Ask<string>("[magenta]Enter your career:[/]");
                        careerId = _careerRepository.GetOrCreateCareer(careerName);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[bold red]Error handling careers:[/] {ex.Message}");
                    AnsiConsole.WriteLine("Using default career instead.");
                    AnsiConsole.WriteLine();
                    PressAnyKey();
                }
            }
            else
            {
                // Manual entry if repository is not available
                var careerName = AnsiConsole.Ask<string>("[magenta]Your career:[/]");
                AnsiConsole.MarkupLine("[grey]Note: Career information saved for reference only in this demo.[/]");
            }
            
            // Location selection
            int cityId = 1; // Default value
            
            if (_locationRepository != null)
            {
                try 
                {
                    // Step 1: Select country
                    var selectedCountry = SelectCountry();
                    if (selectedCountry != null)
                    {
                        // Step 2: Select region
                        var selectedRegion = SelectRegion(selectedCountry.CountryID);
                        if (selectedRegion != null)
                        {
                            // Step 3: Select city
                            var selectedCity = SelectCity(selectedRegion.RegionID);
                            if (selectedCity != null)
                            {
                                cityId = selectedCity.CityID;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[bold red]Error loading locations:[/] {ex.Message}");
                    AnsiConsole.WriteLine("Using default location instead.");
                    AnsiConsole.WriteLine();
                    PressAnyKey();
                }
            }
            else
            {
                // Manual entry if repository is not available
                var countryName = AnsiConsole.Ask<string>("[magenta]Your country:[/]");
                var regionName = AnsiConsole.Ask<string>("[magenta]Your region/state:[/]");
                var cityName = AnsiConsole.Ask<string>("[magenta]Your city:[/]");
                AnsiConsole.MarkupLine("[grey]Note: Location information saved for reference only in this demo.[/]");
            }
            
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

        // Helper methods for location selection
        private Country? SelectCountry()
        {
            if (_locationRepository == null)
                return null;
            
            var countries = _locationRepository.GetAllCountries();
            
            if (countries.Count == 0)
            {
                // If no countries exist, allow creating one
                var countryName = AnsiConsole.Ask<string>("[magenta]Enter your country name:[/]");
                int countryId = _locationRepository.GetOrCreateCountry(countryName);
                return new Country { CountryID = countryId, CountryName = countryName };
            }
            
            // Add option to create a new country
            countries.Add(new Country { CountryID = -1, CountryName = "Create new country..." });
            
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<Country>()
                    .Title("[magenta]Select your country:[/]")
                    .PageSize(Math.Max(3, Math.Min(10, countries.Count)))
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .UseConverter(c => c.CountryName)
                    .AddChoices(countries));
            
            if (selection.CountryID == -1)
            {
                // User chose to create a new country
                var countryName = AnsiConsole.Ask<string>("[magenta]Enter country name:[/]");
                int countryId = _locationRepository.GetOrCreateCountry(countryName);
                return new Country { CountryID = countryId, CountryName = countryName };
            }
            
            return selection;
        }
        
        private campus_love_app.domain.entities.Region? SelectRegion(int countryId)
        {
            if (_locationRepository == null)
                return null;
            
            var regions = _locationRepository.GetRegionsByCountryId(countryId);
            
            if (regions.Count == 0)
            {
                // If no regions exist for this country, allow creating one
                var regionName = AnsiConsole.Ask<string>("[magenta]Enter your state/region:[/]");
                int regionId = _locationRepository.GetOrCreateRegion(regionName, countryId);
                return new campus_love_app.domain.entities.Region { RegionID = regionId, RegionName = regionName, CountryID = countryId };
            }
            
            // Add option to create a new region
            regions.Add(new campus_love_app.domain.entities.Region { RegionID = -1, RegionName = "Create new state/region...", CountryID = countryId });
            
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<campus_love_app.domain.entities.Region>()
                    .Title("[magenta]Select your state/region:[/]")
                    .PageSize(Math.Max(3, Math.Min(10, regions.Count)))
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .UseConverter(r => r.RegionName)
                    .AddChoices(regions));
            
            if (selection.RegionID == -1)
            {
                // User chose to create a new region
                var regionName = AnsiConsole.Ask<string>("[magenta]Enter state/region name:[/]");
                int regionId = _locationRepository.GetOrCreateRegion(regionName, countryId);
                return new campus_love_app.domain.entities.Region { RegionID = regionId, RegionName = regionName, CountryID = countryId };
            }
            
            return selection;
        }
        
        private City? SelectCity(int regionId)
        {
            if (_locationRepository == null)
                return null;
            
            var cities = _locationRepository.GetCitiesByRegionId(regionId);
            
            if (cities.Count == 0)
            {
                // If no cities exist for this region, allow creating one
                var cityName = AnsiConsole.Ask<string>("[magenta]Enter your city:[/]");
                int cityId = _locationRepository.GetOrCreateCity(cityName, regionId);
                return new City { CityID = cityId, CityName = cityName, RegionID = regionId };
            }
            
            // Add option to create a new city
            cities.Add(new City { CityID = -1, CityName = "Create new city...", RegionID = regionId });
            
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<City>()
                    .Title("[magenta]Select your city:[/]")
                    .PageSize(Math.Max(3, Math.Min(10, cities.Count)))
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .UseConverter(c => c.CityName)
                    .AddChoices(cities));
            
            if (selection.CityID == -1)
            {
                // User chose to create a new city
                var cityName = AnsiConsole.Ask<string>("[magenta]Enter city name:[/]");
                int cityId = _locationRepository.GetOrCreateCity(cityName, regionId);
                return new City { CityID = cityId, CityName = cityName, RegionID = regionId };
            }
            
            return selection;
        }

        public void ShowUserProfile(User user, bool isMatchScreen = false, List<User> allProfiles = null, int currentIndex = 0)
        {
            Console.Clear();
            DrawHeader(isMatchScreen ? "Match" : "Profile");
            
            // Create a profile panel
            var profileText = $"[bold]{user.FullName}, {user.Age}[/]";
            var phraseText = $"[grey]Phrase:[/] [italic]{user.ProfilePhrase}[/]";
            
            // Show profile navigation info if we have multiple profiles
            string navigationInfo = "";
            if (!isMatchScreen && allProfiles != null && allProfiles.Count > 1)
            {
                navigationInfo = $"\n[grey]Profile {currentIndex + 1} of {allProfiles.Count}[/]";
            }
            
            var profileContent = new Panel(profileText + "\n" + phraseText + navigationInfo)
                .Border(BoxBorder.Rounded)
                .BorderColor(isMatchScreen ? Color.Yellow : Color.HotPink)
                .Padding(2, 1)
                .Header(isMatchScreen ? "[yellow]IT'S A MATCH![/]" : $"[magenta]{(user.IsVerified ? "✓ " : "")}{user.FullName}[/]");
            
            AnsiConsole.Write(profileContent);

            // Si el perfil está enriquecido, mostrar detalles adicionales
            if (user.HasEnrichedProfile)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold magenta]Enriched Profile[/]");
                
                var enrichedTable = new Table();
                enrichedTable.Border = TableBorder.Rounded;
                enrichedTable.BorderColor(Color.HotPink);
                enrichedTable.HideHeaders();
                enrichedTable.AddColumn(new TableColumn("Field").Width(15));
                enrichedTable.AddColumn(new TableColumn("Value"));
                
                // Only show fields that have information
                if (!string.IsNullOrEmpty(user.ExtendedDescription))
                {
                    enrichedTable.AddRow("[grey]Description[/]", user.ExtendedDescription);
                }
                
                if (!string.IsNullOrEmpty(user.Hobbies))
                {
                    enrichedTable.AddRow("[grey]Hobbies[/]", user.Hobbies);
                }
                
                if (!string.IsNullOrEmpty(user.FavoriteBooks))
                {
                    enrichedTable.AddRow("[grey]Books[/]", user.FavoriteBooks);
                }
                
                if (!string.IsNullOrEmpty(user.FavoriteMovies))
                {
                    enrichedTable.AddRow("[grey]Movies[/]", user.FavoriteMovies);
                }
                
                if (!string.IsNullOrEmpty(user.FavoriteMusic))
                {
                    enrichedTable.AddRow("[grey]Music[/]", user.FavoriteMusic);
                }
                
                // Social media (if available)
                var socialMediaAvailable = false;
                var socialMediaPanel = new Panel("");
                var socialMediaContent = "";
                
                if (!string.IsNullOrEmpty(user.InstagramProfile))
                {
                    socialMediaContent += $"[blue]Instagram:[/] @{user.InstagramProfile}\n";
                    socialMediaAvailable = true;
                }
                
                if (!string.IsNullOrEmpty(user.TwitterProfile))
                {
                    socialMediaContent += $"[cyan]Twitter:[/] @{user.TwitterProfile}\n";
                    socialMediaAvailable = true;
                }
                
                if (!string.IsNullOrEmpty(user.LinkedInProfile))
                {
                    socialMediaContent += $"[blue]LinkedIn:[/] {user.LinkedInProfile}\n";
                    socialMediaAvailable = true;
                }
                
                if (socialMediaAvailable)
                {
                    enrichedTable.AddRow("[grey]Social Media[/]", socialMediaContent);
                }
                
                AnsiConsole.Write(enrichedTable);
            }

            AnsiConsole.WriteLine();
            if (!isMatchScreen)
            {
                // Prepare options based on navigation availability
                var options = new List<string>();
                options.Add("💖 Like");
                options.Add("👎 Dislike");
                
                // Add navigation options if we have multiple profiles
                if (allProfiles != null && allProfiles.Count > 1)
                {
                    if (currentIndex > 0)
                        options.Add("⬅️ Previous profile");
                    
                    if (currentIndex < allProfiles.Count - 1)
                        options.Add("➡️ Next profile");
                }
                
                options.Add("↩️ Back to menu");
                
                // If we're on the profiles screen, show like/dislike options
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[magenta]What would you like to do?[/]")
                        .PageSize(Math.Max(3, Math.Min(10, options.Count)))
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .AddChoices(options));
                
                // Handle the selected option
                if (option == "💖 Like")
                {
                    // Register the like in the database if repository is available
                    if (_userRepository != null && _currentUser != null)
                    {
                        _userRepository.LikeUser(_currentUser.UserID, user.UserID);
                        AnsiConsole.MarkupLine("[green]You liked this profile![/]");
                    }
                    else
                    {
                        // Demo mode or error
                        AnsiConsole.MarkupLine("[green]You liked this profile![/] [grey](Demo mode - like not saved)[/]");
                    }
                    
                    PressAnyKey();
                    
                    // Continue showing other profiles if available
                    if (allProfiles != null && currentIndex < allProfiles.Count - 1)
                    {
                        ShowUserProfile(allProfiles[currentIndex + 1], false, allProfiles, currentIndex + 1);
                    }
                }
                else if (option == "👎 Dislike")
                {
                    // Register the dislike interaction if repository is available
                    if (_userRepository != null && _currentUser != null)
                    {
                        // We'd need to add a DislikeUser method to the repository
                        // For now we'll just show a message
                        AnsiConsole.MarkupLine("[grey]Profile skipped.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[grey]Profile skipped.[/]");
                    }
                    
                    PressAnyKey();
                    
                    // Continue showing other profiles if available
                    if (allProfiles != null && currentIndex < allProfiles.Count - 1)
                    {
                        ShowUserProfile(allProfiles[currentIndex + 1], false, allProfiles, currentIndex + 1);
                    }
                }
                else if (option == "⬅️ Previous profile" && currentIndex > 0)
                {
                    // Show previous profile
                    ShowUserProfile(allProfiles[currentIndex - 1], false, allProfiles, currentIndex - 1);
                }
                else if (option == "➡️ Next profile" && currentIndex < allProfiles.Count - 1)
                {
                    // Show next profile
                    ShowUserProfile(allProfiles[currentIndex + 1], false, allProfiles, currentIndex + 1);
                }
                // No need for else (Back to menu) as we'll just return
            }
            else
            {
                // In the match screen, just show the option to go back
                PressAnyKey();
            }
        }

        public void ShowMatches(List<User> matches, IChatRepository chatRepository)
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
                .AddColumn(new TableColumn("Actions").Centered());

            foreach (var match in matches)
            {
                table.AddRow(
                    $"[bold]{match.FullName}[/]",
                    match.Age.ToString(),
                    $"[link=view:{match.UserID}]View[/] | [link=chat:{match.UserID}]Chat[/]"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Interaction to view a specific profile or return to the menu
            var options = new List<string> { "⬅️ Back to menu" };
            options.AddRange(matches.Select(m => $"View profile #{m.UserID}"));
            options.AddRange(matches.Select(m => $"Chat with #{m.UserID}"));
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to do?[/]")
                    .PageSize(Math.Max(3, Math.Min(10, matches.Count * 2 + 1)))
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(options));
            
            if (choice.StartsWith("View profile #"))
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
            else if (choice.StartsWith("Chat with #"))
            {
                // Extract the selected user ID
                string selectedIdText = choice.Replace("Chat with #", "");
                if (int.TryParse(selectedIdText, out int selectedId) && _currentUser != null)
                {
                    // Get or create a conversation with this user
                    int currentUserId = _currentUser.UserID;
                    var conversation = chatRepository.GetConversationBetweenUsers(currentUserId, selectedId);
                    if (conversation == null)
                    {
                        int conversationId = chatRepository.CreateConversation(currentUserId, selectedId);
                        conversation = chatRepository.GetConversation(conversationId);
                    }
                    
                    if (conversation != null)
                    {
                        // Show the chat conversation
                        ShowChatConversation(conversation, chatRepository);
                    }
                    else
                    {
                        ShowError("Failed to start chat. Please try again.");
                    }
                }
            }
        }

        public void ShowConversations(List<Conversation> conversations, int currentUserId, IChatRepository chatRepository)
        {
            Console.Clear();
            DrawHeader("My Messages");
            
            if (conversations.Count == 0)
            {
                AnsiConsole.MarkupLine("[blue]You don't have any conversations yet.[/]");
                AnsiConsole.MarkupLine("[grey]Start chatting with your matches![/]");
                AnsiConsole.WriteLine();
                PressAnyKey();
                return;
            }
            
            AnsiConsole.MarkupLine($"[grey]You have [blue]{conversations.Count}[/] conversations[/]");
            AnsiConsole.WriteLine();
            
            // Show conversations list
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("With").Centered())
                .AddColumn(new TableColumn("Last Message").Centered())
                .AddColumn(new TableColumn("Unread").Centered())
                .Expand();
                
            foreach (var conversation in conversations)
            {
                // Get the other user in the conversation
                string otherUserName = conversation.User1ID == currentUserId 
                    ? conversation.User2?.FullName ?? "Unknown" 
                    : conversation.User1?.FullName ?? "Unknown";
                
                // Get unread count
                int unreadCount = conversation.GetUnreadCount(currentUserId);
                
                table.AddRow(
                    $"[bold]{otherUserName}[/]",
                    $"{conversation.LastMessageDate.ToString("MM/dd HH:mm")}",
                    unreadCount > 0 ? $"[bold red]{unreadCount}[/]" : "0"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Let user select a conversation
            var options = new List<string> { "⬅️ Back to menu" };
            options.AddRange(conversations.Select(c => {
                string otherUserName = c.User1ID == currentUserId 
                    ? c.User2?.FullName ?? "Unknown" 
                    : c.User1?.FullName ?? "Unknown";
                return $"Chat with {otherUserName}";
            }));
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[blue]Select a conversation:[/]")
                    .PageSize(Math.Max(3, Math.Min(10, conversations.Count + 1)))
                    .HighlightStyle(new Style(foreground: Color.Blue))
                    .AddChoices(options));
            
            if (choice != "⬅️ Back to menu")
            {
                // Extract the selected user name
                string selectedUserName = choice.Replace("Chat with ", "");
                
                // Find the corresponding conversation
                var selectedConversation = conversations.FirstOrDefault(c => 
                    (c.User1ID == currentUserId && c.User2?.FullName == selectedUserName) || 
                    (c.User2ID == currentUserId && c.User1?.FullName == selectedUserName));
                
                if (selectedConversation != null)
                {
                    // Show the chat conversation
                    ShowChatConversation(selectedConversation, chatRepository);
                }
            }
        }
        
        public void ShowChatConversation(Conversation conversation, IChatRepository chatRepository)
        {
            if (_currentUser == null) return;
            
            int currentUserId = _currentUser.UserID;
            int otherUserId = conversation.GetOtherUserID(currentUserId);
            string otherUserName = conversation.User1ID == currentUserId 
                ? conversation.User2?.FullName ?? "Unknown" 
                : conversation.User1?.FullName ?? "Unknown";
            
            bool continueChat = true;
            
            while (continueChat)
            {
                Console.Clear();
                DrawHeader($"Chat with {otherUserName}");
                
                // Get messages for this conversation
                var messages = chatRepository.GetConversationMessages(conversation.ConversationID);
                
                // Mark messages as read
                chatRepository.MarkMessagesAsRead(conversation.ConversationID, currentUserId);
                
                if (messages.Count == 0)
                {
                    AnsiConsole.MarkupLine("[grey]No messages yet. Start the conversation![/]");
                    AnsiConsole.WriteLine();
                }
                else
                {
                    // Show messages using a much simpler approach
                    foreach (var message in messages)
                    {
                        bool isMine = message.SenderID == currentUserId;
                        string senderName = isMine ? "You" : otherUserName;
                        Color messageColor = isMine ? Color.Green : Color.Blue;
                        
                        // Escape markup in the message text to prevent formatting issues
                        string escapedText = Markup.Escape(message.MessageText);
                        
                        // Create a simple table instead of a panel
                        var table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(messageColor)
                            .AddColumn(new TableColumn($"[{messageColor.ToMarkup()}]{senderName}[/] ({message.SentDate.ToString("MM/dd HH:mm")})"));
                        
                        table.AddRow(escapedText);
                        
                        // For my messages, right align the table, for others, left align
                        if (isMine)
                        {
                            AnsiConsole.Write(new Rule().RuleStyle(Style.Plain));
                            AnsiConsole.Write(table.RightAligned());
                        }
                        else
                        {
                            AnsiConsole.Write(table.LeftAligned());
                        }
                        
                        AnsiConsole.WriteLine();
                    }
                }
                
                AnsiConsole.WriteLine();
                
                // Options for the user
                var options = new List<string> 
                {
                    "📝 Send message",
                    "⬅️ Back to conversations"
                };
                
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[blue]What would you like to do?[/]")
                        .PageSize(3)
                        .HighlightStyle(new Style(foreground: Color.Blue))
                        .AddChoices(options));
                
                if (choice == "📝 Send message")
                {
                    try
                    {
                        // Get the message text
                        var messageText = AnsiConsole.Ask<string>("[green]Your message:[/]");
                        
                        if (!string.IsNullOrWhiteSpace(messageText))
                        {
                            // Send the message
                            int messageId = chatRepository.SendMessage(conversation.ConversationID, currentUserId, messageText);
                            
                            if (messageId <= 0)
                            {
                                ShowError("Failed to send message. Please try again.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Error sending message: {ex.Message}");
                    }
                }
                else
                {
                    continueChat = false;
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

        public void ShowUserDetailedStatistics(Dictionary<string, Dictionary<string, string>> categorizedStats)
        {
            Console.Clear();
            DrawHeader("My Statistics");
            
            // Display a welcome message
            AnsiConsole.MarkupLine("[bold magenta]Here are your personal statistics:[/]");
            AnsiConsole.WriteLine();
            
            // Display each category of statistics
            foreach (var category in categorizedStats)
            {
                AnsiConsole.MarkupLine($"[bold underline]{category.Key}[/]");
                
                // Create a table for each category
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(GetColorForCategory(category.Key))
                    .AddColumn(new TableColumn("Metric").LeftAligned())
                    .AddColumn(new TableColumn("Value").RightAligned())
                    .Expand();
                
                foreach (var stat in category.Value)
                {
                    table.AddRow(
                        $"[{GetColorForCategory(category.Key)}]{stat.Key}[/]",
                        $"[bold white]{stat.Value}[/]"
                    );
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }
            
            PressAnyKey();
        }
        
        private Color GetColorForCategory(string category)
        {
            return category switch
            {
                "Basic Stats" => Color.Magenta1,
                "Behavior" => Color.Green,
                "Comparisons" => Color.Gold1,
                _ => Color.Blue
            };
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

        // Method to display and edit the enriched profile
        public void ShowEnrichedProfileEditor(User user)
        {
            Console.Clear();
            DrawHeader("Profile Enrichment");
            
            AnsiConsole.MarkupLine("[bold]Enrich your profile to stand out among other users![/]");
            AnsiConsole.WriteLine();
            
            // Show current profile if it has data
            if (user.HasEnrichedProfile)
            {
                var table = new Table();
                table.AddColumn("Field");
                table.AddColumn("Current Value");
                table.Border = TableBorder.Rounded;
                
                table.AddRow("Extended Description", string.IsNullOrEmpty(user.ExtendedDescription) ? "[grey]Empty[/]" : user.ExtendedDescription);
                table.AddRow("Hobbies", string.IsNullOrEmpty(user.Hobbies) ? "[grey]Empty[/]" : user.Hobbies);
                table.AddRow("Favorite Books", string.IsNullOrEmpty(user.FavoriteBooks) ? "[grey]Empty[/]" : user.FavoriteBooks);
                table.AddRow("Favorite Movies", string.IsNullOrEmpty(user.FavoriteMovies) ? "[grey]Empty[/]" : user.FavoriteMovies);
                table.AddRow("Favorite Music", string.IsNullOrEmpty(user.FavoriteMusic) ? "[grey]Empty[/]" : user.FavoriteMusic);
                table.AddRow("Instagram Profile", string.IsNullOrEmpty(user.InstagramProfile) ? "[grey]Empty[/]" : user.InstagramProfile);
                table.AddRow("Twitter Profile", string.IsNullOrEmpty(user.TwitterProfile) ? "[grey]Empty[/]" : user.TwitterProfile);
                table.AddRow("LinkedIn Profile", string.IsNullOrEmpty(user.LinkedInProfile) ? "[grey]Empty[/]" : user.LinkedInProfile);
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }
            
            // Ask if user wants to edit the profile
            bool editProfile = AnsiConsole.Confirm("Do you want to edit your enriched profile?", true);
            
            if (editProfile)
            {
                user.ExtendedDescription = AnsiConsole.Ask<string>("Extended description (tell more about yourself):", user.ExtendedDescription ?? string.Empty);
                user.Hobbies = AnsiConsole.Ask<string>("Hobbies (comma separated):", user.Hobbies ?? string.Empty);
                user.FavoriteBooks = AnsiConsole.Ask<string>("Favorite books (comma separated):", user.FavoriteBooks ?? string.Empty);
                user.FavoriteMovies = AnsiConsole.Ask<string>("Favorite movies (comma separated):", user.FavoriteMovies ?? string.Empty);
                user.FavoriteMusic = AnsiConsole.Ask<string>("Favorite music (artists or genres, comma separated):", user.FavoriteMusic ?? string.Empty);
                
                // Social media (optional)
                AnsiConsole.MarkupLine("[grey]Social media profiles are optional, leave them blank if you don't want to share them[/]");
                user.InstagramProfile = AnsiConsole.Ask<string>("Instagram username (without @):", user.InstagramProfile ?? string.Empty);
                user.TwitterProfile = AnsiConsole.Ask<string>("Twitter username (without @):", user.TwitterProfile ?? string.Empty);
                user.LinkedInProfile = AnsiConsole.Ask<string>("LinkedIn profile URL:", user.LinkedInProfile ?? string.Empty);
                
                // Save changes to the database
                if (_userRepository != null && _userRepository.UpdateEnrichedProfile(user))
                {
                    user.HasEnrichedProfile = true;
                    ShowSuccess("Enriched profile updated successfully!");
                }
                else
                {
                    ShowError("There was a problem updating your enriched profile.");
                }
            }
            
            PressAnyKey();
        }

        public void ShowRemainingCredits()
        {
            if (_currentUser == null || _userRepository == null) return;
            
            int userId = _currentUser.UserID;
            int creditsRemaining = _userRepository.GetRemainingCredits(userId);
            
            Console.Clear();
            DrawHeader("Daily Credits");
            
            AnsiConsole.MarkupLine("[bold]Daily Like Credits[/]");
            AnsiConsole.WriteLine();
            
            // Create a panel to display the credits
            var panel = new Panel($"[bold]You have [green]{creditsRemaining}[/] credits remaining today[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Padding(2, 1);
            
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            
            AnsiConsole.MarkupLine("[grey]Credits are reset daily. Each like costs 1 credit.[/]");
            AnsiConsole.MarkupLine("[grey]Use them wisely to connect with people you're interested in![/]");
            
            PressAnyKey();
        }
    }
} 