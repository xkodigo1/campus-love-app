using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface IUserRepository
    {
        void RegisterUser(User user);
        List<User> GetAvailableProfiles(int userId);
        void LikeUser(int fromUserId, int toUserId);
        List<User> GetMatches(int userId);
        User? GetUserById(int userId);
    }
}