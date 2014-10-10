using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARd.Search.DataBaseWork
{
    public static class DataBaseClass
    {
        public static SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
        public static SqlConnection ch_php_dbc = new SqlConnection(Properties.Settings.Default.ch_phpConnectionString);

        static SqlDataReader reader;

        public static SqlDataReader DoProcedureCHD1(string sql, Dictionary<string, object> param)
        {
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            if (param.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in param)
                {
                    sc.Parameters.AddWithValue(kvp.Key, kvp.Value.ToString());
                }
            }
            ch_d_1_dbc.Open();
            try
            {
                reader = sc.ExecuteReader();
            }
            catch (Exception exc)
            {
                
            }
            return reader;
        }

        public static void CloseReader()
        {
            if(reader != null)
                reader.Close();
            return;
        }

        public static void CloseConnections()
        {
            ch_d_1_dbc.Close();
            ch_php_dbc.Close();
        }
    }
}
