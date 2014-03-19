using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    public class FormaClass
    {
        public int Index { get; set; }
        public Nullable<bool> Selected { get; set; }
        int Id;
        public string FormaName { get; set; }
        int MpnId;

        public FormaClass(int Id, int MpnId, string Forma, Nullable<bool> IsSelect)
        {
            this.Id = Id;
            this.FormaName = Forma;
            this.Selected = IsSelect;
            this.MpnId = MpnId;
        }

        public int GetId()
        {
            return this.Id;
        }
        public int GetMpnId()
        {
            return this.MpnId;
        }
        public override string ToString()
        {
            return this.FormaName;
        }
        public Nullable<bool> IsSelect()
        {
            return this.Selected;
        }
    }
}
