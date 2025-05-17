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
    }
}