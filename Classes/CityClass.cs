using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARd.Search
{
    public class CityClass
    {
        int id;
        string cname;

        public CityClass(int id, string name) 
        {
            this.id = id;
            this.cname = name;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.cname;
        }

    }
}
