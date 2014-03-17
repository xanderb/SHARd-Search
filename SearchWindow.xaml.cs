using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using System.IO;

namespace CCsearch
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int ListPage = 0;
        public const int FormaPage = 1;
        public const int FinalPage = 2;
        //SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
        //SqlConnection ch_php_dbc = new SqlConnection(Properties.Settings.Default.ch_phpConnectionString);
        TextWriterTraceListener DListener = new TextWriterTraceListener(@"d:\work\LOGS\debugCCSearch.txt");


        #region lists
        ObservableCollection<MPNClass> mpns = new ObservableCollection<MPNClass>();
        ObservableCollection<InterClass> inters = new ObservableCollection<InterClass>();
        ObservableCollection<FarmClass> farms = new ObservableCollection<FarmClass>();
        ObservableCollection<CityClass> cities = new ObservableCollection<CityClass>();
        ObservableCollection<ComplexClass> complexies = new ObservableCollection<ComplexClass>();
        ObservableCollection<MPNClass> sinonims = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> analogs = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> filters = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> selectedMpns = new ObservableCollection<MPNClass>();
        ObservableCollection<FormaClass> forma = new ObservableCollection<FormaClass>();
        Dictionary<int, ObservableCollection<FormaClass>> formas = new Dictionary<int, ObservableCollection<FormaClass>>();
        Dictionary<int, ObservableCollection<MpClass>> mps = new Dictionary<int, ObservableCollection<MpClass>>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            selectedMpns.CollectionChanged += new NotifyCollectionChangedEventHandler(SelectedListChangeItem);
        }

        private void SearchWindow_Initialized(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.IsDebug)
            {
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

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("Время предзагрузки данных - {0}", TimeInterval.ToString()));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}", TimeInterval.ToString() ); }));
            }
        }
        private void DebugMode(Action act, string Label)
        {
            lock (this)
            {
                Debug.Listeners.Add(DListener);
                Debug.AutoFlush = true;

                DateTime StartTime = DateTime.Now;

                Dispatcher.Invoke(act);

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("\r\nВремя предзагрузки данных - {0}, act= {1}", TimeInterval.ToString(), Label));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}, act= {1}", TimeInterval.ToString(), Label); }));
            }
        }

        private void SearchWindow_Loaded(object sender, RoutedEventArgs e)
        {    
            this.SearchWindow.Width = (SystemParameters.PrimaryScreenWidth / SystemParameters.CaretWidth) - 250;
            this.SearchWindow.Height = (SystemParameters.PrimaryScreenHeight / SystemParameters.CaretWidth) - 40;
        }

        private void PreloadSearch()
        {
            Analog.ItemsSource = analogs;
            Sinonim.ItemsSource = sinonims;
            MPNList.ItemsSource = mpns;
            MPN.ItemsSource = mpns;
            Inter.ItemsSource = inters;
            InterList.ItemsSource = inters;
            Farm.ItemsSource = farms;
            FarmList.ItemsSource = farms;
            City.ItemsSource = cities;
            Address.ItemsSource = complexies;
            FilterList.ItemsSource = filters;
            SelectedMPN.ItemsSource = selectedMpns;

            Thread MpnThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACmpn(); }, "MPN"); }));
            Thread InterThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACInter(); }, "Inter"); }));
            Thread FarmThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACFarm(); }, "Pharm"); }));
            Thread CityThread = new Thread(new ThreadStart(delegate { DebugMode(() => { PreloadACCity(); }, "City"); }));

            MpnThread.Start();
            InterThread.Start();
            FarmThread.Start();
            CityThread.Start();

        }

        #region Preloads
        private void PreloadACmpn()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id ORDER BY name";
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
                    bool isSinonim = Convert.ToBoolean(data["is_sinonim"].ToString());
                    bool isAnalog = Convert.ToBoolean(data["is_analog"].ToString());
                    mpns.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog));
                }
            }
            finally
            {
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
                    inters.Add(new InterClass(id, name, true));
                }
            }
            finally
            {
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
                    farms.Add(new FarmClass(id, name, true));
                }
            }
            finally
            {
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
                    DebugText.Text += "\r\n Нажата клавиша Enter " + String.Format("Выбрано- {0}.{1}", selectedCity.GetID(), selectedCity.ToString());
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
            FilterList.Visibility = Visibility.Hidden;
        }
        private void Inter_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Visible;
            BorderInter.Visibility = Visibility.Visible;
            FarmList.Visibility = Visibility.Hidden;
            BorderFarm.Visibility = Visibility.Hidden;
            MPNList.Visibility = Visibility.Hidden;
            BorderMPN.Visibility = Visibility.Hidden;
            FilterList.Visibility = Visibility.Hidden;

        }
        private void Farm_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Hidden;
            BorderInter.Visibility = Visibility.Hidden;
            FarmList.Visibility = Visibility.Visible;
            BorderFarm.Visibility = Visibility.Visible;
            MPNList.Visibility = Visibility.Hidden;
            BorderMPN.Visibility = Visibility.Hidden;
            FilterList.Visibility = Visibility.Hidden;

        }
        private void Filter_GotFocus(object sender, RoutedEventArgs e)
        {
            InterList.Visibility = Visibility.Hidden;
            BorderInter.Visibility = Visibility.Hidden;
            FarmList.Visibility = Visibility.Hidden;
            BorderFarm.Visibility = Visibility.Hidden;
            MPNList.Visibility = Visibility.Hidden;
            BorderMPN.Visibility = Visibility.Visible;
            FilterList.Visibility = Visibility.Visible;
        }
        

        
        #endregion

        #region list events
        private void FilterTextBoxWork()
        {
            filters.Clear();
            int CollectionCount = mpns.Count;
            if (mpns.Count > 0)
            {
                for (int i = 0; i < CollectionCount; i++)
                {
                    string Text = mpns[i].ToString().ToLower();
                    if (Text.Contains(Filter.Text.ToLower()))
                    {
                        filters.Add(mpns[i]);
                    }
                }
            }
            FilterList.SelectedIndex = 0;
        }
        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            DebugMode(() => { FilterTextBoxWork(); }, "Формирование листинга фильтра");
        }
        private void FilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filters.Count > 0)
            {
                MPNClass SelItem = (MPNClass)FilterList.SelectedItem;
                Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { if (LocalCheck.IsChecked == true) LocalSinonimsAnalogsAction(SelItem); else SinonimsAnalogsAction(SelItem); }, "Выбор в листинге фильтра"); }));
                SelThread.SetApartmentState(ApartmentState.STA);
                SelThread.Start();
            }
        }
        private void MPNList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass SelItem = (MPNClass)MPNList.SelectedItem;
            Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { if (LocalCheck.IsChecked == true) LocalSinonimsAnalogsAction(SelItem); else SinonimsAnalogsAction(SelItem); }, "Листинг Наименований"); }));
            SelThread.SetApartmentState(ApartmentState.STA);
            SelThread.Start();
        }
        private void InterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InterClass SelItem = (InterClass)InterList.SelectedItem;
            Thread SelThread = new Thread(
                new ThreadStart(
                    delegate
                    {
                        DebugMode(() => { SinonimsAction(SelItem); }, "Листинг Междунар.Наим.");
                    }
                    )
                    );
            SelThread.SetApartmentState(ApartmentState.STA);
            SelThread.Start();
        }
        private void FarmList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FarmClass SelItem = (FarmClass)FarmList.SelectedItem;
            Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { AnalogsAction(SelItem); }, "Листинг Фарм.Аналогов"); }));
            SelThread.SetApartmentState(ApartmentState.STA);
            SelThread.Start();
        }
        private void SelectedListChangeItem(object sender, NotifyCollectionChangedEventArgs arg)
        {
            DebugText.Text += String.Format("\r\n Изменен список выбранных продуктов");
            NewFormaTab(arg);
        }
        #endregion

        #region local actions for list events
        public void LocalSinonimsAnalogsAction(MPNClass SelItem)
        {
            int InterId = SelItem.GetInterID();
            int FarmId = SelItem.GetFarmID();
            if (inters.Count > 0 && farms.Count > 0)
            {
                sinonims.Clear();
                analogs.Clear();
                foreach(MPNClass item in mpns)
                {
                    if (item.GetInterID() == InterId && item.IsSinonim() == true)
                    {
                        sinonims.Add(item);
                    }
                    if (item.GetFarmID() == FarmId && item.IsAnalog() == true)
                    {
                        analogs.Add(item);
                    }
                }
            }
        }
        #endregion

        #region actions for list events
        public void SinonimsAnalogsAction(MPNClass SelItem)
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);

            //Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\n {0}, {1}, {2}", SelItem.ToString(), SelItem.GetInterID(), SelItem.GetFarmID()); }));
            int MpnId = SelItem.GetID();
            int InterId = SelItem.GetInterID();
            int FarmId = SelItem.GetFarmID();
            string sql_sinonim = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id  WHERE mpn.international_name_id = @inter  AND i.in_sinonim_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
            string sql_farm_analog = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog  FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.pharmacological_group_id = @farm AND f.pg_analog_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
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
                    bool isSinonim = Convert.ToBoolean(data["is_sinonim"].ToString());
                    bool isAnalog = Convert.ToBoolean(data["is_analog"].ToString());
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { sinonims.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog)); }));
                }
            }
            finally
            {
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
                    bool isSinonim = Convert.ToBoolean(data_a["is_sinonim"].ToString());
                    bool isAnalog = Convert.ToBoolean(data_a["is_analog"].ToString());
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog)); }));
                }
            }
            finally
            {
                ch_d_1_dbc.Dispose();
                data_a.Close();
            }
        }
        public void SinonimsAction(InterClass Inter)
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql_sinonim = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog   FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id  WHERE mpn.international_name_id = @inter  AND i.in_sinonim_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
            SqlCommand sc = new SqlCommand(sql_sinonim, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@inter", Inter.GetID());
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
                    bool isSinonim = Convert.ToBoolean(data["is_sinonim"].ToString());
                    bool isAnalog = Convert.ToBoolean(data["is_analog"].ToString());
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { sinonims.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog)); }));
                }
            }
            finally
            {
                ch_d_1_dbc.Close();
                data.Close();
            }
        }
        public void AnalogsAction(FarmClass Pharm)
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql_farm_analog = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog    FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.pharmacological_group_id = @farm AND f.pg_analog_flag = 1 ORDER BY mpn.medical_product_name_name ASC";
            SqlCommand sc_a = new SqlCommand(sql_farm_analog, ch_d_1_dbc);
            sc_a.Parameters.AddWithValue("@farm", Pharm.GetID());
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
                    bool isSinonim = Convert.ToBoolean(data_a["is_sinonim"].ToString());
                    bool isAnalog = Convert.ToBoolean(data_a["is_analog"].ToString());
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog)); }));
                }
            }
            finally
            {
                ch_d_1_dbc.Dispose();
                data_a.Close();
            }
        }
        #endregion

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ++MainTabs.SelectedIndex;
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (BackButton != null && NextButton != null)
                {
                    int TabsCount = MainTabs.Items.Count;
                    if (MainTabs.SelectedIndex == 0)
                    {
                        BackButton.IsEnabled = false;
                        NextButton.IsEnabled = true;
                    }
                    else if (MainTabs.SelectedIndex == (TabsCount - 1))
                    {
                        BackButton.IsEnabled = true;
                        NextButton.IsEnabled = false;
                    }
                    else
                    {
                        BackButton.IsEnabled = true;
                        NextButton.IsEnabled = true;
                    }
                }
            }
            catch(Exception except)
            {
                MessageBox.Show(except.Message);
            }
            
            //DebugText.Text += String.Format("\r\n Выбранная вкладка - {0}", MainTabs.SelectedIndex);
        }

        private void SelectedMPNSwitcher_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMPN.Visibility == Visibility.Hidden)
                SelectedMPN.Visibility = Visibility.Visible;
            else
                SelectedMPN.Visibility = Visibility.Hidden;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            --MainTabs.SelectedIndex;
        }

        private void MPNList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)MPNList.SelectedItem;
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void FilterList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)FilterList.SelectedItem;
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void Sinonim_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)Sinonim.SelectedItem;
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void Analog_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)Analog.SelectedItem;
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        #region wpf controls work

        private void NewFormaTab(NotifyCollectionChangedEventArgs arg)
        {
            TabItem NewTab = new TabItem();
            MPNClass newMpn = (MPNClass)arg.NewItems[0];
            NewTab.Header = String.Format("{0}", newMpn.ToString());
            GenerateFormaForm(newMpn, NewTab);
            FormaTabs.Items.Add(NewTab);
            NewTab.IsSelected = true;
            MainTabs.SelectedIndex = FormaPage;
        }
        private void GenerateFormaForm(MPNClass MpnForma, TabItem TI)
        {
            //Grid FormaGrid = new Grid();
            //ColumnDefinition cd = new ColumnDefinition();
            //GridLengthConverter glc = new GridLengthConverter();
            //cd.Width = (GridLength)glc.ConvertFromString("2*");
            //FormaGrid.ColumnDefinitions.Add(cd);
            //FormaGrid.ColumnDefinitions.Add(cd);
            //WrapPanel Wrapper = new WrapPanel();

            //FormaGrid.Children.Add(Wrapper);

            //Grid generator
            //GridLengthConverter glConverter = new GridLengthConverter();

            //Grid FormaGrid = new Grid();
            //ColumnDefinition column1 = new ColumnDefinition();
            //column1.Width = (GridLength)glConverter.ConvertFromString("2*");
            //ColumnDefinition column2= new ColumnDefinition();
            //column2.Width = (GridLength)glConverter.ConvertFromString("4*");
            //FormaGrid.ColumnDefinitions.Add(column1);
            //FormaGrid.ColumnDefinitions.Add(column2);

            ////Buttons Generator
            //ThicknessConverter ticConvert = new ThicknessConverter();

            //Button FormaPlus = new Button();
            //FormaPlus.Width = 25;
            //FormaPlus.Height = 25;
            //FormaPlus.Name = "FormaAll";
            //FormaPlus.VerticalAlignment = VerticalAlignment.Top;
            //FormaPlus.HorizontalAlignment = HorizontalAlignment.Left;
            //FormaPlus.Content = "+";
            //FormaPlus.Margin = (Thickness)ticConvert.ConvertFromString("5, 5, 0, 0");
            //Grid.SetColumn(FormaPlus, 0);
            //Button FormaMinus = new Button();
            //FormaMinus.Width = 25;
            //FormaMinus.Height = 25;
            //FormaMinus.Name = "FormaEmpty";
            //FormaMinus.VerticalAlignment = VerticalAlignment.Top;
            //FormaMinus.HorizontalAlignment = HorizontalAlignment.Left;
            //FormaMinus.Content = "-";
            //FormaMinus.Margin = (Thickness)ticConvert.ConvertFromString("35, 5, 0, 0");
            //Grid.SetColumn(FormaMinus, 0);
            //Button FormaSearch = new Button();
            //FormaSearch.Width = 120;
            //FormaSearch.Height = 25;
            //FormaSearch.Name = "FormaSearch";
            //FormaSearch.VerticalAlignment = VerticalAlignment.Top;
            //FormaSearch.HorizontalAlignment = HorizontalAlignment.Left;
            //FormaSearch.Content = "Поиск продуктов";
            //FormaSearch.Margin = (Thickness)ticConvert.ConvertFromString("65, 5, 0, 0");
            //Grid.SetColumn(FormaSearch, 0);


            //Button MpPlus = new Button();
            //MpPlus.Width = 25;
            //MpPlus.Height = 25;
            //MpPlus.Name = "MpAll";
            //MpPlus.VerticalAlignment = VerticalAlignment.Top;
            //MpPlus.HorizontalAlignment = HorizontalAlignment.Left;
            //MpPlus.Content = "+";
            //MpPlus.Margin = (Thickness)ticConvert.ConvertFromString("5, 5, 0, 0");
            //Grid.SetColumn(MpPlus, 1);
            //Button MpMinus = new Button();
            //MpMinus.Width = 25;
            //MpMinus.Height = 25;
            //MpMinus.Name = "MpEmpty";
            //MpMinus.VerticalAlignment = VerticalAlignment.Top;
            //MpMinus.HorizontalAlignment = HorizontalAlignment.Left;
            //MpMinus.Content = "+";
            //MpMinus.Margin = (Thickness)ticConvert.ConvertFromString("35, 5, 0, 0");
            //Grid.SetColumn(MpMinus, 1);
            //Button MpSearch = new Button();
            //MpSearch.Width = 120;
            //MpSearch.Height = 25;
            //MpSearch.Name = "MpSearch";
            //MpSearch.VerticalAlignment = VerticalAlignment.Top;
            //MpSearch.HorizontalAlignment = HorizontalAlignment.Left;
            //MpSearch.Content = "Поиск аптек";
            //MpSearch.Margin = (Thickness)ticConvert.ConvertFromString("65, 5, 0, 0");
            //Grid.SetColumn(MpSearch, 1);

            //FormaGrid.Children.Add(FormaPlus);FormaGrid.Children.Add(FormaMinus);FormaGrid.Children.Add(FormaSearch);
            //FormaGrid.Children.Add(MpPlus);FormaGrid.Children.Add(MpMinus);FormaGrid.Children.Add(MpSearch);
            
            ////ListViews generations
            //ListView FormaList = new ListView();
            //FormaList.Name = "FormaGrid";
            //FormaList.Margin = (Thickness)ticConvert.ConvertFromString("10,40,10,10");
            //Grid.SetColumn(FormaList, 0);
            //FormaList.HorizontalAlignment = HorizontalAlignment.Stretch;
            //FormaList.VerticalAlignment = VerticalAlignment.Stretch;
            
            //GridView gv = new GridView();

            //GridViewColumn gridColumn1 = new GridViewColumn();
            //gridColumn1.Header = "Флаг";
            //gridColumn1.Width = 30;
            //CheckBox cb = new CheckBox();

            //gv.Columns.Add(gridColumn1);
            //FormaList.View = gv;
            
            UserControl UC = new SecondPage();
            try
            {
                /*//But1.Content = MpnForma.ToString();
                FormaGrid = XamlClone<Grid>(ControlGrid);
                ListView list = (ListView)FormaGrid.FindName("FormaGrid");
                
                args.ControlObject = list;
                DynamicMfShow(args);
                ////**События*/
                //list.MouseLeftButtonUp += (cBu, arg) =>
                //    {
                //        FormaListViewSelected(list, args);
                //    };
                ////***********//
                
                ListView LV = (ListView)UC.FindName("FormaGrid");
                FormEventArgs args = new FormEventArgs();
                args.DataObject = MpnForma;
                args.ControlObject = LV;
                DynamicMfShow(args);
                /*CB.Checked += (par1, par2) =>
                    {
                        FormaListViewSelected(CB, args);
                    };*/
            }
            catch (Exception except)
            {
                DebugText.Text += "\r\n" + except.Message + " " + except.Data["line"];
            }
            finally
            {
               TI.Content = UC;
            }
        }
        #endregion
        private void DynamicMfShow(FormEventArgs e)
        {
            MPNClass mpn = (MPNClass)e.DataObject;
            ListView LV = (ListView)e.ControlObject;
            int index = FormaTabs.Items.Count;
            ObservableCollection<FormaClass> formaSource = new ObservableCollection<FormaClass>();
            //Забираем инфу о формах выпуска из базы
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT DISTINCT(mf.medical_form_id) as mf_id, mf.medical_form_name as mf_name, mp.medical_product_name_id as mpn_id FROM medical_form mf WITH (NOLOCK) INNER JOIN medical_product mp ON mp.medical_form_id = mf.medical_form_id WHERE mp.medical_product_name_id = @mpn_id";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@mpn_id", mpn.GetID());
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            try
            {
                //Dispatcher.BeginInvoke(new ThreadStart(delegate { analogs.Clear(); }));
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["mf_id"].ToString()); 
                    int MpnId = Convert.ToInt32(data["mpn_id"].ToString());
                    string FormaName = data["mf_name"].ToString();
                    formaSource.Add(new FormaClass(id, MpnId, FormaName, true));
                }
                formas.Add(index, formaSource); //Добавляем источник форм в общий словарь источников форм
            }
            finally
            {
                LV.Items.Clear();
                LV.ItemsSource = formas[index]; //Присоединяем источник к списку из общего словаря*/


                ch_d_1_dbc.Dispose();
                data.Close();
            }
        }
        private void FormaListViewSelected(object sender, FormEventArgs e)
        {
            ListView lv = (ListView) sender;
            FormaClass fc = (FormaClass)lv.SelectedItem;
            string sel = fc.Selected.ToString();
            MessageBox.Show(String.Format("{0} - {1}", fc.FormaName, sel));
        }
        public void FormaListViewSelected(object sender, RoutedEventArgs args)
        {
            CheckBox cb = sender as CheckBox;
            string ind = cb.Tag.ToString();
            MessageBox.Show(ind);
        }
        #region Xaml cloning method
        public T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            StringReader stringReader = new StringReader(savedObject);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }
        #endregion
        
        
        //private void DynamicControl_Click(object sender, RoutedEventArgs e)
        //{
        //    Button testButton = new Button();
        //    int count = TestDynamicArea.Children.Count;
        //    testButton.Content = String.Format("Йа кнопко {0}!", ++count);
        //    testButton.Name = String.Format("Button{0}", count);
        //    testButton.Style = (Style)this.Resources["RulledButtons"];
        //    TestDynamicArea.Children.Add(testButton);
        //}

        //private void TestGetButton_Click(object sender, RoutedEventArgs e)
        //{
        //    int index = Convert.ToInt32(TestIndexButton.Text);
        //    Button gettedBut = (Button)TestDynamicArea.Children[index];
        //    MessageBox.Show(gettedBut.Name);
        //}

        //private void TestRemoveItem_Click(object sender, RoutedEventArgs e)
        //{
        //    TestDynamicArea.Children.Remove(TestDynamicArea.Children[1]);
        //}
        

        

    }

    
}
