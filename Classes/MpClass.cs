using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class MpClass
    {
        public bool IsSelect;
        public string MpName;
        public string FormaName;
        public string Description1;
        public string Description2;
        int MpnId;
        int MpId;
        int FormaId;

        public MpClass(int MpId, int MpnId, int FormaId, string MpName, string FormaName, string Description1, string Description2, bool IsSelect)
        {
            this.MpId = MpId;
            this.MpnId = MpnId;
            this.FormaId = FormaId;
            this.MpName = MpName;
            this.FormaName = FormaName;
            this.Description1 = Description1;
            this.Description2 = Description2;
            this.IsSelect = IsSelect;
        }

        public override string ToString()
        {
            return this.MpName+" "+this.FormaName+" "+this.Description1+" "+this.Description2;
        }
    }
}
