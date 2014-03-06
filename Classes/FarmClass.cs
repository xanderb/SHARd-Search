using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class FarmClass
    {
        int id;
        string fname;
        bool is_analog;

        public FarmClass(int id, string name, bool is_analog)
        {
            this.id = id;
            this.fname = name;
            this.is_analog = is_analog;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.fname;
        }

        public bool IsAnalog()
        {
            return this.is_analog;
        }
    }
}
