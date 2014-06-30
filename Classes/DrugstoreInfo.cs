using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SHARd.Search
{
    public class DrugstoreInfo :INotifyPropertyChanged
    {
        private Nullable<bool> _Select;
        public int Index { get; set; }
        public Nullable<bool> Selected 
        {
            get
            {
                return this._Select;
            }
            set
            {
                if (value != this._Select)
                {
                    this._Select = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public int PageNum { get; set; }
        public bool HalfLife { get; set; }
        public string ShelfLive { get; set; }
        public int DsId { get; set; }
        public string DsName { get; set; }
        public string DDVoice { get; set; }
        public int DDId { get; set; }
        public string DDName { get; set; }
        public string DDStr { get; set; }
        public string DDTel { get; set; }
        public string DDAddress { get; set; }
        public int MpId { get; set; }
        public string DDNote { get; set; }
        public int MpnId { get; set; }
        public string MpnName { get; set; }
        public string MpnNote { get; set; }
        public int MfId { get; set; }
        public string MfName { get; set; }
        public string MpStr { get; set; }
        public string DsMpStr { get; set; }
        public int InterId { get; set; }
        public string InterName { get; set; }
        public string PrName { get; set; }
        public bool PrVal { get; set; }
        public bool OrderFlag { get; set; }
        public long DsComplexPriority { get; set; }
        public bool SearchCount { get; set; }
        public long DsMpId { get; set; }
        public double DsMpPrice { get; set; }
        public int PackingId { get; set; }
        public string PackingName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string DsMpPresenceDate { get; set; }
        public string DsMpDate { get; set; }
        public string DsMpPresenceFullDate { get; set; }
        public int LoadStrLekId { get; set; }
        public string LoadStrLekName { get; set; }
        public int LoadStrLekChecks { get; set; }
        public int PgId { get; set; }
        public string PgName { get; set; }
        public string Voice { get; set; }
        public Nullable<bool> IsVoiced { get; set; }
        public int DDCount { get; set; }
        public int FindKey { get; set; }

        public event PropertyChangedEventHandler PropertyChanged; 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public DrugstoreInfo( IDataRecord rdr )
        {
            if (rdr != null)
            {
                if (rdr["page_num"] != null)
                    this.PageNum = Convert.ToInt32(rdr["page_num"].ToString());
                if (rdr["hl"] != null)
                    this.HalfLife = Convert.ToBoolean(Convert.ToInt32(rdr["hl"].ToString()));
                if (rdr["sl"] != null)
                    this.ShelfLive = rdr["sl"].ToString();
                if (rdr["ds_id"] != null)
                    this.DsId = Convert.ToInt32(rdr["ds_id"].ToString());
                if (rdr["ds_name"] != null)
                    this.DsName = rdr["ds_name"].ToString();
                if (rdr["dd_voice"] != null)
                    this.DDVoice = rdr["dd_voice"].ToString();
                if (rdr["dd_id"] != null)
                    this.DDId = Convert.ToInt32(rdr["dd_id"].ToString());
                if (rdr["dd_name"] != null)
                    this.DDName = rdr["dd_name"].ToString();
                if (rdr["dd_str"] != null)
                    this.DDStr = rdr["dd_str"].ToString();
                if (rdr["dd_tel"] != null)
                    this.DDTel = rdr["dd_tel"].ToString();
                if (rdr["dd_address"] != null)
                    this.DDAddress = rdr["dd_address"].ToString();
                if (rdr["mp_id"] != null)
                    this.MpId = Convert.ToInt32(rdr["mp_id"].ToString());
                if (rdr["dd_note"] != null)
                    this.DDNote = rdr["dd_note"].ToString();
                if (rdr["mpn_id"] != null)
                    this.MpnId = Convert.ToInt32(rdr["mpn_id"].ToString());
                if (rdr["mpn_name"] != null)
                    this.MpnName = rdr["mpn_name"].ToString(); 
                if (rdr["mpn_note"] != null)
                    this.MpnNote = rdr["mpn_note"].ToString();
                if (rdr["mf_id"] != null)
                    this.MfId = Convert.ToInt32(rdr["mf_id"].ToString());
                if (rdr["mf_name"] != null)
                    this.MfName = rdr["mf_name"].ToString();
                if (rdr["mp_str"] != null)
                    this.MpStr = rdr["mp_str"].ToString(); 
                if (rdr["ds_mp_str"] != null)
                    this.DsMpStr = rdr["ds_mp_str"].ToString();
                if (rdr["inter_id"] != null)
                    this.InterId = Convert.ToInt32(rdr["inter_id"].ToString());
                if (rdr["inter_name"] != null)
                    this.InterName = rdr["inter_name"].ToString();
                if (rdr["pr_name"] != null)
                    this.PrName = rdr["pr_name"].ToString();
                if (rdr["pr_val"] != null)
                    this.PrVal = Convert.ToBoolean(rdr["pr_val"]);
                if (rdr["order_flag"] != null)
                    this.OrderFlag = Convert.ToBoolean(rdr["order_flag"]);
                if (rdr["ds_complex_priority"] != null)
                    this.DsComplexPriority = Convert.ToInt64(rdr["ds_complex_priority"].ToString());
                if (rdr["search_count"] != null)
                    this.SearchCount = Convert.ToBoolean(rdr["search_count"]);
                if (rdr["ds_mp_id"] != null && rdr["ds_mp_id"].ToString() != "")
                    this.DsMpId = Convert.ToInt64(rdr["ds_mp_id"].ToString());
                if (rdr["ds_mp_price"] != null && rdr["ds_mp_price"].ToString() != "")
                    this.DsMpPrice = Convert.ToDouble(rdr["ds_mp_price"].ToString());
                if (rdr["p_id"] != null && rdr["p_id"].ToString() != "")
                    this.PackingId = Convert.ToInt32(rdr["p_id"].ToString());
                if (rdr["p_name"] != null)
                    this.PackingName = rdr["p_name"].ToString();
                if (rdr["country_id"] != null)
                    this.CountryId = Convert.ToInt32(rdr["country_id"].ToString());
                if (rdr["country_name"] != null)
                    this.CountryName = rdr["country_name"].ToString();
                if (rdr["ds_mp_pr_tsdate"] != null)
                    this.DsMpPresenceDate = rdr["ds_mp_pr_tsdate"].ToString();
                if (rdr["ds_mp_tsdate"] != null)
                    this.DsMpDate = rdr["ds_mp_tsdate"].ToString();
                if (rdr["ds_mp_presence_tsdate"] != null)
                    this.DsMpPresenceFullDate = rdr["ds_mp_presence_tsdate"].ToString();
                if (rdr["load_str_lek_id"] != null && rdr["load_str_lek_id"].ToString() != "")
                {
                    string str = rdr["load_str_lek_id"].ToString();
                    this.LoadStrLekId = Convert.ToInt32(str);
                }
                if (rdr["load_str_lek_name"] != null)
                    this.LoadStrLekName = rdr["load_str_lek_name"].ToString();
                if (rdr["load_str_lek_checks"] != null && rdr["load_str_lek_checks"].ToString() != "")
                {
                    string str = rdr["load_str_lek_checks"].ToString();
                    this.LoadStrLekChecks = Convert.ToInt32(str);
                }
                if (rdr["pg_id"] != null && rdr["pg_id"].ToString() != "")
                {
                    string str = rdr["pg_id"].ToString();
                    this.PgId = Convert.ToInt32(str);
                }
                if (rdr["pg_name"] != null)
                    this.PgName = rdr["pg_name"].ToString();
                if (rdr["voice"] != null)
                {
                    this.Voice = rdr["voice"].ToString();
                    if (rdr["voice"].ToString() != "")
                        this.IsVoiced = true;
                    else
                        this.IsVoiced = false;
                }
                if (rdr["dd_count"] != null && rdr["dd_count"].ToString() != "")
                    this.DDCount = Convert.ToInt32(rdr["dd_count"].ToString());
                if (rdr["find_key"] != null && rdr["find_key"].ToString() != "")
                    this.FindKey = Convert.ToInt32(rdr["find_key"].ToString()); 
            }
            Selected = false;
        }
    }
}
