using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    class ComplexClass
    {
        int id;
        string name;
        CityClass city;

        public ComplexClass(int id, string name, CityClass city)
        {
            this.id = id;
            this.name = name;
            this.city = city;
        }

        public int GetID()
        {
            return this.id;
        }

        public CityClass GetCity()
        {
            return this.city;
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
