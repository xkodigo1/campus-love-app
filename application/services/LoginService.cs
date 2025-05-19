using System;
using System.Collections.Generic;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;
using System.Security.Cryptography;
using System.Text;

namespace campus_love_app.application.services
{
    public class LoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAccountRepository _accountRepository;

        public LoginService(IUserRepository userRepository, IUserAccountRepository accountRepository)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
        }

        public (User?, UserAccount?) Login(string usernameOrEmail, string password)
        {
            // Check if the credentials are valid
            UserAccount? account = _accountRepository.Authenticate(usernameOrEmail, password);
            
            if (account != null)
            {
                // Get the user information
                User? user = _userRepository.GetUserById(account.UserID);
                
                if (user != null)
                {
                    return (user, account);
                }
            }
            
            // Authentication failed
            return (null, null);
        }
        
        public (User, UserAccount) Register(string email, string username, string password, User userInfo)
        {
            // Check if username or email already exists
            if (_accountRepository.UsernameExists(username))
            {
                throw new ArgumentException("Username already exists");
            }
            
            if (_accountRepository.EmailExists(email))
            {
                throw new ArgumentException("Email already exists");
            }
            
            // Register the user in the repository - usando CreateUser en lugar de RegisterUser
            User? createdUser = _userRepository.CreateUser(userInfo);
            
            if (createdUser == null || createdUser.UserID <= 0)
            {
                throw new Exception("Failed to create user profile");
            }
            
            // Hash the password using SHA256 (consider using a more secure method in production)
            string passwordHash = HashPassword(password);
            
            // Create the account
            UserAccount account = new UserAccount
            {
                UserID = createdUser.UserID,
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                LastLoginDate = DateTime.Now
            };
            
            // Register the account
            _accountRepository.RegisterUserAccount(account);
            
            if (account.AccountID <= 0)
            {
                throw new Exception("Failed to create user account");
            }
            
            return (createdUser, account);
        }
        
        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 