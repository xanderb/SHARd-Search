using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class InterClass
    {
        int id;
        string InterName;
        bool is_sinonim;

        public InterClass(int id, string name, bool is_sinonim)
        {
            this.id = id;
            this.InterName = name;
            this.is_sinonim = is_sinonim;
        }
        public InterClass() { }
        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.InterName;
        }
        public bool IsSinonim()
        {
            return this.is_sinonim;
        }
    }
}
