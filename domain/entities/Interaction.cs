using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class Interaction
    {
        public int InteractionID { get; set; }
        public int FromUserID { get; set; }
        public int ToUserID { get; set; }
        public bool IsLike { get; set; }
        public DateTime InteractionDate { get; set; }
    }
}