using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    class FarmClass
    {
        int id;
        string fname;

        public FarmClass(int id, string name)
        {
            this.id = id;
            this.fname = name;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.fname;
        }

    }
}
