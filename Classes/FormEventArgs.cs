using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCsearch
{
    class FormEventArgs : EventArgs
    {
        public object DataObject { get; set; }
        public object ControlObject { get; set; }
    }
}
