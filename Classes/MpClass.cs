using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class MpClass
    {
        public int Index { get; set; }
        public Nullable<bool> Selected { get; set; }
        public string MpName { get; set; }
        public string FormaName { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
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
            this.Selected = IsSelect;
        }

        public override string ToString()
        {
            return this.MpName+" "+this.FormaName+" "+this.Description1+" "+this.Description2;
        }

        public int GetMpId()
        {
            return this.MpId;
        }
        public int GetMpnId() 
        {
            return this.MpnId;
        }
        public int GetFormaId()
        {
            return this.FormaId;
        }
        public object GetValueByName(string Option)
        {
            switch (Option)
            {
                case "FormaName":
                    return this.FormaName;
                case "MpName":
                    return this.MpName;
                case "Op1":
                    return this.Description1;
                case "Op2":
                    return this.Description2;
                default:
                    return false;
            }
        }
    }
}
