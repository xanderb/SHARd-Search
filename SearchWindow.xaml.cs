using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace CCsearch
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
        SqlConnection ch_php_dbc = new SqlConnection(Properties.Settings.Default.ch_phpConnectionString);*/
        TextWriterTraceListener DListener = new TextWriterTraceListener(@"d:\work\LOGS\debugCCSearch.txt");

        #region lists
        ObservableCollection<MPNClass> mpns = new ObservableCollection<MPNClass>();
        ObservableCollection<InterClass> inters = new ObservableCollection<InterClass>();
        ObservableCollection<FarmClass> farms = new ObservableCollection<FarmClass>();
        ObservableCollection<CityClass> cities = new ObservableCollection<CityClass>();
        ObservableCollection<ComplexClass> complexies = new ObservableCollection<ComplexClass>();
        ObservableCollection<MPNClass> sinonims = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> analogs = new ObservableCollection<MPNClass>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchWindow_Initialized(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.IsDebug)
            {
                //DebugMode();
                PreloadSearch();
            }
            else
            {
                PreloadSearch();
            }
        }

        private void DebugMode(Action act)
        {
            lock (this)
            {
                Debug.Listeners.Add(DListener);
                Debug.AutoFlush = true;

                DateTime StartTime = DateTime.Now;

                Dispatcher.Invoke(act);
                //PreloadSearch();

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("Время предзагрузки данных - {0}", TimeInterval.ToString()));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя предзагрузки данных - {0}, act= {1}", TimeInterval.ToString(), act.Target.ToString() ); }));
            }
        }

        private void SearchWindow_Loaded(object sender, RoutedEventArgs e)
        {    
            this.SearchWindow.Width = (SystemParameters.PrimaryScreenWidth / SystemParameters.CaretWidth) - 250;
            this.SearchWindow.Height = (SystemParameters.PrimaryScreenHeight / SystemParameters.CaretWidth) - 40;
        }

        private void PreloadSearch()
        {
            Thread MpnThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACmpn(); }); }));
            Thread InterThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACInter(); }); }));
            Thread FarmThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACFarm(); }); }));
            Thread CityThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACCity(); }); }));

            MpnThread.Start();
            InterThread.Start();
            FarmThread.Start();
            CityThread.Start();
            
            /*PreloadACmpn();
            PreloadACInter();
            PreloadACFarm();
            PreloadACCity();*/
        }

        #region Preloads
        private void PreloadACmpn()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id ORDER BY name";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            
            try
            {
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["name"].ToString();
                    string interName = data["inter_name"].ToString();
                    int interId = Convert.ToInt32(data["inter_id"].ToString());
                    int farmId = Convert.ToInt32(data["farm_id"].ToString());
                    string farmName = data["farm_name"].ToString();
                    mpns.Add(new MPNClass(id, name, interId, interName, farmId, farmName));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { MPNList.ItemsSource = mpns; MPN.ItemsSource = mpns; }));
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACInter()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT international_name_name as iname, international_name_id as id FROM international_name WITH (NOLOCK) WHERE in_sinonim_flag = 1 ORDER BY international_name_name ASC";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();

            try
            {
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["iname"].ToString();
                    inters.Add(new InterClass(id, name));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Inter.ItemsSource = inters; InterList.ItemsSource = inters; }));
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACFarm()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT a.pharmacological_group_id as id, a.pharmacological_group_name as fname FROM pharmacological_group a WITH (NOLOCK) WHERE a.pg_analog_flag = 1 ORDER BY a.pharmacological_group_name ASC";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();

            try
            {
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["fname"].ToString();
                    farms.Add(new FarmClass(id, name));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Farm.ItemsSource = farms; FarmList.ItemsSource = farms; }));
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACCity()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT a.area_name as cname, a.area_id as id FROM area a WITH (NOLOCK) WHERE a.area_SortCode > 0 ORDER BY a.area_SortCode ASC";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();

            try
            {
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["cname"].ToString();
                    cities.Add(new CityClass(id, name));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { City.ItemsSource = cities; }));
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACComplex()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            complexies.Clear();
            CityClass selectedCity = (CityClass)City.SelectedItem;
            string sql = "SELECT a.complex_name as name, a.complex_id as id FROM complex a WITH (NOLOCK) WHERE a.area_id = @city ORDER BY a.complex_code ASC";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@city", selectedCity.GetID());
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();

            try
            {
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["name"].ToString();
                    complexies.Add(new ComplexClass(id, name, selectedCity));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Address.ItemsSource = complexies; }));
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        #endregion

        #region combobox events
        private void MPN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass sel_elem = (MPNClass)MPN.SelectedItem;
            if (sel_elem != null)
            {
                int index = MPN.SelectedIndex;
                MPNList.SelectedIndex = index;
                MPNList.ScrollIntoView(MPNList.SelectedItem);
            }

        }
        private void Inter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InterClass SelElem = (InterClass)Inter.SelectedItem;
            if (SelElem != null)
            {
                int index = Inter.SelectedIndex;
                InterList.SelectedIndex = index;
                InterList.ScrollIntoView(InterList.SelectedItem);
            }
        }

        private void Farm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FarmClass SelElem = (FarmClass)Farm.SelectedItem;
            if (SelElem != null)
            {
                int index = Farm.SelectedIndex;
                FarmList.SelectedIndex = index;
                FarmList.ScrollIntoView(FarmList.SelectedItem);
            }
        }

        private void MPN_KeyUp(object sender, KeyEventArgs e)
        {
            MPNClass sel_elem = (MPNClass)MPN.SelectedItem;
            if (e.Key == Key.Return && sel_elem != null)
                DebugText.Text += "\r\n Нажата клавиша Enter" + String.Format("Выбрано- {0}.{1}", sel_elem.GetID(), sel_elem.ToString());
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CityClass selectedCity = (CityClass)City.SelectedItem;
            if (selectedCity != null)
            {
                Address.IsEnabled = true;
                PreloadACComplex();
            }
            else
            {
                Address.IsEnabled = false;
                complexies.Clear();  
            }
        }

        private void City_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                CityClass selectedCity = (CityClass)City.SelectedItem;
                if (selectedCity != null)
                {
                    DebugText.Text += "\r\n Нажата клавиша Enter" + String.Format("Выбрано- {0}.{1}", selectedCity.GetID(), selectedCity.ToString());
                    Address.IsEnabled = true;
                    PreloadACComplex();
                }
                else
                {
                    Address.IsEnabled = false;
                    complexies.Clear();
                }
            }
        }
        
        private void MPN_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Hidden;
            BorderInter.Visibility = Visibility.Hidden;
            FarmList.Visibility = Visibility.Hidden;
            BorderFarm.Visibility = Visibility.Hidden;
            MPNList.Visibility = Visibility.Visible;
            BorderMPN.Visibility = Visibility.Visible;
        }
        private void Inter_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Visible;
            BorderInter.Visibility = Visibility.Visible;
            FarmList.Visibility = Visibility.Hidden;
            BorderFarm.Visibility = Visibility.Hidden;
            MPNList.Visibility = Visibility.Hidden;
            BorderMPN.Visibility = Visibility.Hidden;
        }
        private void Farm_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Hidden;
            BorderInter.Visibility = Visibility.Hidden;
            FarmList.Visibility = Visibility.Visible;
            BorderFarm.Visibility = Visibility.Visible;
            MPNList.Visibility = Visibility.Hidden;
            BorderMPN.Visibility = Visibility.Hidden;
        }
        #endregion

        #region list events
        private void MPNList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass SelItem = (MPNClass)MPNList.SelectedItem;
            Thread SelThread = new Thread(new ThreadStart(delegate { SinonimsAnalogsAction(SelItem); }));
            SelThread.Start();
        }

        #endregion

        #region action for list events
        public void SinonimsAnalogsAction(MPNClass SelItem)
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);

            Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\n{0}, {1}, {2}", SelItem.ToString(), SelItem.GetInterID(), SelItem.GetFarmID()); }));
            int MpnId = SelItem.GetID();
            int InterId = SelItem.GetInterID();
            int FarmId = SelItem.GetFarmID();
            string sql_sinonim = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id  WHERE mpn.international_name_id = @inter  AND i.in_sinonim_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
            string sql_farm_analog = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name  FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.pharmacological_group_id = @farm AND f.pg_analog_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
            //Sinonims
            SqlCommand sc = new SqlCommand(sql_sinonim, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@inter", InterId);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();

            try
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { sinonims.Clear(); }));
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["name"].ToString();
                    string interName = data["inter_name"].ToString();
                    int interId = Convert.ToInt32(data["inter_id"].ToString());
                    int farmId = Convert.ToInt32(data["farm_id"].ToString());
                    string farmName = data["farm_name"].ToString();
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { sinonims.Add(new MPNClass(id, name, interId, interName, farmId, farmName)); }));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Synonim.ItemsSource = sinonims; }));
                ch_d_1_dbc.Close();
                data.Close();
            }
            //Analogs
            SqlCommand sc_a = new SqlCommand(sql_farm_analog, ch_d_1_dbc);
            sc_a.Parameters.AddWithValue("@farm", FarmId);
            ch_d_1_dbc.Open();
            SqlDataReader data_a = sc_a.ExecuteReader();

            try
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Clear(); }));
                while (data_a.Read())
                {
                    int id = Convert.ToInt32(data_a["id"].ToString());
                    string name = data_a["name"].ToString();
                    string interName = data_a["inter_name"].ToString();
                    int interId = Convert.ToInt32(data_a["inter_id"].ToString());
                    int farmId = Convert.ToInt32(data_a["farm_id"].ToString());
                    string farmName = data_a["farm_name"].ToString();
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Add(new MPNClass(id, name, interId, interName, farmId, farmName)); }));
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Analogs.ItemsSource = analogs; }));
                ch_d_1_dbc.Dispose();
                data_a.Close();
            }
        }
        #endregion




    }

    
}
