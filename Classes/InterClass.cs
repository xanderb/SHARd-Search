using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    class InterClass
    {
        int id;
        string InterName;

        public InterClass(int id, string name)
        {
            this.id = id;
            this.InterName = name;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.InterName;
        }
    }
}
