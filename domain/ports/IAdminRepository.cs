using System.Collections.Generic;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface IAdminRepository
    {
        Administrator GetAdminByUsername(string username);
        Administrator GetAdminByEmail(string email);
        bool ValidateAdmin(string username, string password);
        List<User> GetAllUsers();
        User GetUserById(int userId);
        bool VerifyUser(int userId);
        bool BanUser(int userId);
        bool UnbanUser(int userId);
        bool DeleteUser(int userId);
        List<User> SearchUsers(string searchTerm);
    }
} 