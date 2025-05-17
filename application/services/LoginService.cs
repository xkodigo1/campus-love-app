using System;
using campus_love_app.domain.entities;
using campus_love_app.domain.ports;

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

        public (User? user, UserAccount? account) Login(string usernameOrEmail, string password)
        {
            // Validate credentials
            if (!_accountRepository.ValidateCredentials(usernameOrEmail, password))
            {
                return (null, null);
            }
            
            // Retrieve the account
            UserAccount? account;
            if (usernameOrEmail.Contains("@"))
            {
                account = _accountRepository.GetAccountByEmail(usernameOrEmail);
            }
            else
            {
                account = _accountRepository.GetAccountByUsername(usernameOrEmail);
            }
            
            if (account == null)
            {
                return (null, null);
            }
            
            // Update last login date
            _accountRepository.UpdateLastLoginDate(account.AccountID);
            
            // Get the associated user
            User? user = _userRepository.GetUserById(account.UserID);
            
            return (user, account);
        }

        public (User user, UserAccount account) Register(
            string email, 
            string username, 
            string password, 
            User user)
        {
            // Check if username or email already exists
            if (_accountRepository.UsernameExists(username))
            {
                throw new Exception("Username already exists");
            }
            
            if (_accountRepository.EmailExists(email))
            {
                throw new Exception("Email already exists");
            }
            
            // Register the user first
            _userRepository.RegisterUser(user);
            
            // Create the account
            var account = new UserAccount
            {
                UserID = user.UserID,
                Email = email,
                Username = username
            };
            
            // Register the account
            _accountRepository.RegisterAccount(account, password);
            
            return (user, account);
        }
    }
} 