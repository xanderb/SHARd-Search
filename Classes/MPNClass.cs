using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARd.Search
{
    public class MPNClass
    {
        int id;
        string name;
        int interId;
        string interName;
        int farmId;
        string farmName;
        bool isSinonim;
        bool isAnalog;
        public int Index { get; set; }
        public int InterIndex { get; set; }
        public int FarmIndex { get; set; }

        public MPNClass(int id, string name, int inter_id, string inter_name, int farm_id, string farm_name, bool is_sinonim, bool is_analog)
        {
            this.id = id;
            this.name = name;
            this.interId = inter_id;
            this.interName = inter_name;
            this.farmId = farm_id;
            this.farmName = farm_name;
            this.isSinonim = is_sinonim;
            this.isAnalog = is_analog;
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
        public bool IsSinonim()
        {
            return this.isSinonim;
        }
        public bool IsAnalog()
        {
            return this.isAnalog;
        }
    }
}
