using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARd.Search
{
    public class SMSClass
    {
        int    id;
        string sms_message_text;
        string sms_message_number;
        string sms_message_date;
        public int Index { get; set; }
        public bool selected { get; set; }

        public SMSClass(int id, string sms_text, string sms_number, string sms_date)
        {
            this.id = id;
            this.sms_message_text = sms_text;
            this.sms_message_number = sms_number;
            this.sms_message_date = sms_date;
        }

        public int GetID()
        {
            return this.id;
        }

        public override string ToString()
        {
            return this.sms_message_text;
        }

        public string GetNumber()
        {
            return this.sms_message_number;
        }

        public string GetDate()
        {
            return this.sms_message_date;
        }
    }
}
