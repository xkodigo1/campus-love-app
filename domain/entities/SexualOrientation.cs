using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace campus_love_app.domain.entities
{
    public class SexualOrientation
    {
        public int OrientationID { get; set; }
        public string OrientationName { get; set; } = string.Empty;
    }
}