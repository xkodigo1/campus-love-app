using System;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface IUserAccountRepository
    {
        void RegisterAccount(UserAccount account, string plainPassword);
        UserAccount? GetAccountByUsername(string username);
        UserAccount? GetAccountByEmail(string email);
        UserAccount? GetAccountByUserId(int userId);
        bool ValidateCredentials(string usernameOrEmail, string password);
        void UpdateLastLoginDate(int accountId);
        void ChangePassword(int accountId, string newPassword);
        bool UsernameExists(string username);
        bool EmailExists(string email);
        
        // Renamed method alias for RegisterAccount
        void RegisterUserAccount(UserAccount account);
        
        // New method that combines getting account and validating credentials
        UserAccount? Authenticate(string usernameOrEmail, string password);
    }
} 