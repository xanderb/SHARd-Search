using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class MPNClass
    {
        int id;
        string name;
        int interId;
        string interName;
        int farmId;
        string farmName;

        public MPNClass(int id, string name, int inter_id, string inter_name, int farm_id, string farm_name)
        {
            this.id = id;
            this.name = name;
            this.interId = inter_id;
            this.interName = inter_name;
            this.farmId = farm_id;
            this.farmName = farm_name;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.name;
        }

        public string GetInterName()
        {
            return this.interName;
        }

        public int GetInterID()
        {
            return this.interId;
        }

        public int GetFarmID()
        {
            return this.farmId;
        }

        public string GetFarmName()
        {
            return this.farmName;
        }
    }
}
