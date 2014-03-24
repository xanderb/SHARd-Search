using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace CCsearch
{
    /// <summary>
    /// Логика взаимодействия для SecondPage.xaml
    /// </summary>
    public partial class SecondPage : UserControl
    {
        public MainWindow Main { get; set; }

        public SecondPage()
        {
            InitializeComponent();
        }
        public SecondPage(MainWindow Main)
        {
            this.Main = Main;
            InitializeComponent();
        }

        private void FormaCheckbox_Click_1(object sender, RoutedEventArgs e)
        {
            //CheckBox Cb = sender as CheckBox;
            //int CbInd = Convert.ToInt32(Cb.Tag.ToString());
            //Dispatcher.BeginInvoke(new ThreadStart(delegate
            //{
            //    if (Main.FormaTabs != null && Main.FormaTabs.SelectedIndex != -1)
            //    {
            //        int TabInd = Main.FormaTabs.SelectedIndex;
            //        MessageBox.Show(String.Format("Индекс таба - {0}, индекс чекбокса - {1}, значение - {2}", TabInd, CbInd, Main.formas[TabInd][CbInd].ToString()));
            //    }
            //}));
        }

        private void FormaAll_Click(object sender, RoutedEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;
            foreach (FormaClass forma in Main.formas[TabIndex])
            {
                forma.Selected = true;
            }
            FormaGrid.Items.Refresh();
        }

        private void FormaEmpty_Click(object sender, RoutedEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;
            foreach (FormaClass forma in Main.formas[TabIndex])
            {
                forma.Selected = false;
            }
            FormaGrid.Items.Refresh();
        }

        private void MpAll_Click(object sender, RoutedEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;
            foreach (MpClass mp in Main.mps[TabIndex])
            {
                mp.Selected = true;
            }
            MpGrid.Items.Refresh();
        }

        private void MpEmpty_Click(object sender, RoutedEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;
            foreach (MpClass mp in Main.mps[TabIndex])
            {
                mp.Selected = false;
            }
            MpGrid.Items.Refresh();
        }

        private void MpSearch_Click(object sender, RoutedEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;
            ObservableCollection<FormaClass> Forma = Main.formas[TabIndex];
            int[] formaIdArray = new int[Forma.Count];
            for(int i = 0; i < Forma.Count; i++)
            {
                if((bool)Forma[i].Selected)
                    formaIdArray[i] = Forma[i].GetId();
            }
            string FormaIn = String.Join(",", formaIdArray);
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = string.Format("SELECT mpn.medical_product_name_name as mpn, mpn.medical_product_name_id as mpn_id, mf.medical_form_name as mfn, mp.medical_product_id as mp_id, mp.medical_product_str as mp_str1, mp.medical_product_str2 as mp_str2, mp.medical_form_id as mf_id FROM medical_product mp WITH (NOLOCK) INNER JOIN medical_product_name mpn WITH (NOLOCK) ON mp.medical_product_name_id = mpn.medical_product_name_id INNER JOIN medical_form mf WITH (NOLOCK) ON mp.medical_form_id = mf.medical_form_id WHERE mp.medical_product_name_id = @mpn_id AND mf.medical_form_id IN ({0}) ORDER BY mpn.medical_product_name_name ASC, mp.medical_form_id ASC, mp.medical_product_str ASC", FormaIn);
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@mpn_id", Forma[0].GetMpnId());
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            ObservableCollection<MpClass> mpSource = new ObservableCollection<MpClass>();
            try
            {
                //Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Clear(); }));
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["mp_id"].ToString());
                    int MpnId = Convert.ToInt32(data["mpn_id"].ToString());
                    int FormaId = Convert.ToInt32(data["mf_id"].ToString());
                    string FormaName = data["mfn"].ToString();
                    string MpnName = data["mpn"].ToString();
                    string str1 = data["mp_str1"].ToString();
                    string str2 = data["mp_str2"].ToString();

                    MpClass mpObj = new MpClass(id, MpnId, FormaId, MpnName, FormaName, str1, str2, true);
                    int ind = mpSource.Count;
                    mpObj.Index = ind;
                    mpSource.Add(mpObj);
                }
                Main.mps[TabIndex].Clear();
                Main.mps[TabIndex] = mpSource;
            }
            finally
            {
                //MpGrid.Items.Clear();
                MpGrid.ItemsSource = Main.mps[TabIndex]; //Присоединяем источник к списку из общего словаря*/
                MpGrid.Items.Refresh();
                ch_d_1_dbc.Dispose();
                data.Close();
            }
        }
    }
}
