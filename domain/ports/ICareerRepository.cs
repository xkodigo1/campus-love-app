using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface ICareerRepository
    {
        List<Career> GetAllCareers();
        int GetOrCreateCareer(string careerName);
    }
}