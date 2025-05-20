using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User GetUserById(int userId);
        User? CreateUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(int userId);
        List<User> GetAvailableProfiles(int currentUserId);
        void LikeUser(int likerId, int likedId);
        List<User> GetMatches(int userId);
        Dictionary<string, Dictionary<string, string>> GetAllUserStatistics(int userId);
        
        // Credit management methods
        int GetRemainingCredits(int userId);
        bool UseCredit(int userId);
        void ResetCreditsIfNeeded(int userId);
        
        // Enriched profile methods
        bool UpdateEnrichedProfile(User user);
    }
}