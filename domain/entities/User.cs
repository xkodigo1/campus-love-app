using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public int GenderID { get; set; }
        public int CareerID { get; set; }
        public int OrientationID { get; set; }
        public string ProfilePhrase { get; set; } = string.Empty;
        public int MinPreferredAge { get; set; }
        public int MaxPreferredAge { get; set; }
        public bool IsVerified { get; set; }
        public int CityID { get; set; }
        
        // Campos para enriquecimiento de perfil
        public string ExtendedDescription { get; set; } = string.Empty;
        public string Hobbies { get; set; } = string.Empty;
        public string FavoriteBooks { get; set; } = string.Empty;
        public string FavoriteMovies { get; set; } = string.Empty;
        public string FavoriteMusic { get; set; } = string.Empty;
        public string InstagramProfile { get; set; } = string.Empty;
        public string TwitterProfile { get; set; } = string.Empty;
        public string LinkedInProfile { get; set; } = string.Empty;
        public bool HasEnrichedProfile { get; set; } = false;
    }
}