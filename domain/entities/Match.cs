using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class Match
    {
        public int MatchID { get; set; }
        public int User1ID { get; set;}
        public int User2ID { get; set;}
        public DateTime MatchDate { get; set; }
    }
}