using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Data;
using System.IO;


namespace SHARd.Search
{
    /// <summary>
    /// Логика взаимодействия для SearchMainWindow.xaml
    /// </summary>
    public partial class SearchMainWindow : Window
    {
        public const int ListPage = 0;
        public const int FormaPage = 1;
        public const int FinalPage = 2;
        public const int LoadingStep = 100;
        public const int LoadingStepBig = 1000;
        //SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
        //SqlConnection ch_php_dbc = new SqlConnection(Properties.Settings.Default.ch_phpConnectionString);
        TextWriterTraceListener DListener = new TextWriterTraceListener(Properties.Settings.Default.log_directory.ToString() + @"debugSHARd.Search.txt");
        public FileLog FLog = new FileLog("");
        private BackgroundWorker worker = new BackgroundWorker();
        delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        public delegate void AutoAnswerDelegate(List<int> DrugstoreIds); //делегат для события автоответа со списком ID выбранных аптек

        public event AutoAnswerDelegate onAutoAnswer; //событие автоответа
        
        public int FirmId = 0;
        public int UserId = 713;
        private int CPUCores;
        private int CPULogicalCores;
        private string CPUName;

        #region Collections
        //ObservableCollection<MPNClass> mpns = new ObservableCollection<MPNClass>();
        //ObservableCollection<InterClass> inters = new ObservableCollection<InterClass>();
        //ObservableCollection<FarmClass> farms = new ObservableCollection<FarmClass>();
        //ObservableCollection<CityClass> cities = new ObservableCollection<CityClass>();
        //ObservableCollection<ComplexClass> complexies = new ObservableCollection<ComplexClass>();
        //ObservableCollection<MPNClass> sinonims = new ObservableCollection<MPNClass>();
        //ObservableCollection<MPNClass> analogs = new ObservableCollection<MPNClass>();
        //ObservableCollection<MPNClass> filters = new ObservableCollection<MPNClass>();
        //ObservableCollection<MPNClass> selectedMpns = new ObservableCollection<MPNClass>();
        //ObservableCollection<FormaClass> forma = new ObservableCollection<FormaClass>();

        ObservableCollection<MPNClass> mpns;
        ObservableCollection<InterClass> inters;
        ObservableCollection<FarmClass> farms;
        ObservableCollection<CityClass> cities;
        ObservableCollection<ComplexClass> complexies = new ObservableCollection<ComplexClass>();
        ObservableCollection<MPNClass> sinonims = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> analogs = new ObservableCollection<MPNClass>();
        ObservableCollection<MPNClass> filters;
        ObservableCollection<MPNClass> selectedMpns = new ObservableCollection<MPNClass>();
        ObservableCollection<FormaClass> forma = new ObservableCollection<FormaClass>();
        public ObservableCollection<DrugstoreInfo> finals = new ObservableCollection<DrugstoreInfo>();
        public Dictionary<int, ObservableCollection<FormaClass>> formas = new Dictionary<int, ObservableCollection<FormaClass>>();
        public Dictionary<int, ObservableCollection<MpClass>> mps = new Dictionary<int, ObservableCollection<MpClass>>();
        DataSet CommonData = new DataSet("ch_d_1");
        DataTable MPN_DT; 
        #endregion

        public SearchMainWindow()
        {
            InitializeComponent();
            selectedMpns.CollectionChanged += new NotifyCollectionChangedEventHandler(SelectedListChangeItem);
            GetCPUNumberOfCores();
            if (Directory.Exists(@"\\10.123.7.2\spravka\Logs"))
            {
                FLog = new FileLog(@"\\10.123.7.2\spravka\Logs");
            }
            
            SetUserId(713); //TEST логов
            DebugText.Text += String.Format("Имя машины: {3}. Процессор: {0}\r\nКол-во физических ядер: {1}, кол-во логических ядер: {2}", CPUName, CPUCores, CPULogicalCores, Environment.MachineName);
            FLog.Log(String.Format("Имя машины: {3}. Процессор: {0}\r\nКол-во физических ядер: {1}, кол-во логических ядер: {2}", CPUName, CPUCores, CPULogicalCores, Environment.MachineName));
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            MPN_DT = CommonData.Tables.Add("MPN");
            onAutoAnswer += testAutoAnswerMessage;
            if (CPUCores > 1)
            {
                MPN_DT.RowChanged += new DataRowChangeEventHandler(MPN_DT_RowChanged);
            }
            if (CPUCores < 2)
            {
                MpnBar.IsIndeterminate = true;
                InterBar.IsIndeterminate = true;
                FarmBar.IsIndeterminate = true;
                CityBar.IsIndeterminate = true;
                InterLoadingText.Visibility = Visibility.Hidden;
                MpnLoadingText.Visibility = Visibility.Hidden;
                FarmLoadingText.Visibility = Visibility.Hidden;
                CityLoadingText.Visibility = Visibility.Hidden;
            }
        }

        void MPN_DT_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (this.MpnBar.Value <= MPN_DT.Rows.Count - 1)
                {
                    this.MpnBar.Value++;
                    this.MpnLoadingText.Text = String.Format("{0} / {1}", MPN_DT.Rows.Count - 1, MpnBar.Maximum);
                }
            });
        }

        public void GetCPUNumberOfCores()
        {
            ManagementObjectSearcher search = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach( ManagementObject Obj in search.Get()) 
            {
                CPUCores = Convert.ToInt32(Obj["NumberOfCores"].ToString());
                CPULogicalCores = Convert.ToInt32(Obj["NumberOfLogicalProcessors"].ToString());
                CPUName = Obj["Name"].ToString();
            }
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
            FLog.Log(String.Format("Локальный режим = {0}", LocalCheck.IsChecked.ToString()));
        }

        public void DebugMode(Action act)
        {
            lock (this)
            {
                Debug.Listeners.Add(DListener);
                Debug.AutoFlush = true;

                DateTime StartTime = DateTime.Now;

                Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Render);

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("Время предзагрузки данных - {0}", TimeInterval.ToString()));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}", TimeInterval.ToString()); DebugText.ScrollToEnd(); }));
                FLog.Log(String.Format("Время работы - {0}", TimeInterval.ToString()));
            }
        }
        public void DebugMode(bool flag, Action act, string Label)
        {
            //lock (this)
            //{
                Debug.Listeners.Add(DListener);
                Debug.AutoFlush = true;

                DateTime StartTime = DateTime.Now;

                act();

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("\r\nВремя предзагрузки данных - {0}, act= {1}", TimeInterval.ToString(), Label));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}, act= {1}", TimeInterval.ToString(), Label); DebugText.ScrollToEnd(); }));
                lock (this)
                {
                    FLog.Log(String.Format("Время работы - {0}, act= {1}", TimeInterval.ToString(), Label));
                }
            //}
        }
        public object DebugMode(Func<bool> act, string Label, int action)
        {
            Debug.Listeners.Add(DListener);
            Debug.AutoFlush = true;

            DateTime StartTime = DateTime.Now;

            object ret = act();

            DateTime EndTime = DateTime.Now;
            TimeSpan TimeInterval = EndTime.Subtract(StartTime);
            Debug.WriteLine(String.Format("\r\nВремя предзагрузки данных - {0}, act= {1}", TimeInterval.ToString(), Label));
            Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}, act= {1}", TimeInterval.ToString(), Label); DebugText.ScrollToEnd(); }));
            lock (this)
            {
                FLog.Log(String.Format("Время работы - {0}, act= {1}", TimeInterval.ToString(), Label));
            }
            return ret;
        }
        public void DebugMode(Action act, string Label)
        {
            lock (this)
            {
                Debug.Listeners.Add(DListener);
                Debug.AutoFlush = true;

                DateTime StartTime = DateTime.Now;

                Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Render);

                DateTime EndTime = DateTime.Now;
                TimeSpan TimeInterval = EndTime.Subtract(StartTime);
                Debug.WriteLine(String.Format("\r\nВремя предзагрузки данных - {0}, act= {1}", TimeInterval.ToString(), Label));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { DebugText.Text += String.Format("\r\nВремя работы - {0}, act= {1}", TimeInterval.ToString(), Label); DebugText.ScrollToEnd(); }));
                FLog.Log(String.Format("Время работы - {0}, act= {1}", TimeInterval.ToString(), Label));
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
            /*MPNList.ItemsSource = mpns;
            MPN.ItemsSource = mpns;
            Inter.ItemsSource = inters;
            InterList.ItemsSource = inters;
            Farm.ItemsSource = farms;
            FarmList.ItemsSource = farms;
            City.ItemsSource = cities;*/
            Address.ItemsSource = complexies;
            //FilterList.ItemsSource = filters;
            SelectedMPN.ItemsSource = selectedMpns;

            BackgroundWorker worker1 = new BackgroundWorker();
            /*BackgroundWorker worker2 = new BackgroundWorker();
            BackgroundWorker worker3 = new BackgroundWorker();*/

            worker1.WorkerReportsProgress = true;
           /* worker2.WorkerReportsProgress = true;
            worker3.WorkerReportsProgress = true;*/

            worker1.WorkerSupportsCancellation = true;
           /* worker2.WorkerSupportsCancellation = true;
            worker3.WorkerSupportsCancellation = true;*/

            worker1.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e) { WorkEnd(sender, e); };
            /*worker2.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e) { WorkEnd(sender, e); };
            worker3.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e) { WorkEnd(sender, e); };
            */
            worker1.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
      
                Parallel.Invoke(
                    () => { DebugMode(true, () => { PreloadACFarm(); }, "Pharm"); },
                    () => { DebugMode(true, () => { PreloadACCity(); }, "City"); },
                    () => { DebugMode(true, () => { PreloadACmpn(); }, "MPN"); },
                    () => { DebugMode(true, () => { PreloadACInter(); }, "Inter"); }
                );
     
                

            };
            //worker2.DoWork += delegate(object sender, DoWorkEventArgs e)
            //{
            //    DebugMode(true, () => { PreloadACmpn(); }, "MPN");
            //}; 
            //worker3.DoWork += delegate(object sender, DoWorkEventArgs e)
            //{
            //    DebugMode(true, () => { PreloadACInter(); }, "Inter");
            //};
            worker1.RunWorkerAsync();
            //worker2.RunWorkerAsync();
            //worker3.RunWorkerAsync();
        }
        private void WorkEnd(object sender, RunWorkerCompletedEventArgs e)
        {
            {
                if (e.Cancelled == true)
                {
                    Dispatcher.Invoke(() => { DebugText.Text += String.Format("\r\nЗагрузка отменена"); });
                    LoadingWrap.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                }
                else if (e.Error != null)
                {
                    Dispatcher.Invoke(() => { DebugText.Text += String.Format("\r\nОшибка загрузки: "); });
                    LoadingWrap.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    Dispatcher.Invoke(() => { DebugText.Text += String.Format("\r\nУспешно загружено"); FLog.Log(String.Format("Успешно загружено")); });
                    /*if (InterBar.Value == InterBar.Maximum && FarmBar.Value == FarmBar.Maximum && MpnBar.Value == MpnBar.Maximum && CityBar.Value == CityBar.Maximum)
                    {*/
                        LoadingWrap.Visibility = Visibility.Hidden;
                        MainGrid.Visibility = Visibility.Visible;
                    //}
                }
            };
        }

        #region Preloads
        private void PreloadACmpn()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(MpnBar.SetValue);
            List<MPNClass> MpnList = new List<MPNClass>();
            /*
             * Получаем общее кол-во записей в БД для прогрессбара
             */
            string sqlCount = "SELECT COUNT(*) as count FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i WITH(NOLOCK) ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f WITH(NOLOCK) ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.mpn_PriceOff_Flag = 0";
            int rowCount = 0;
            SqlCommand sc = new SqlCommand(sqlCount, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            data.Read();
            rowCount = Convert.ToInt32(data["count"].ToString());
            data.Close();
            ch_d_1_dbc.Close();


            string sql = "SELECT mpn.medical_product_name_name as name, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, i.in_sinonim_flag as is_sinonim, f.pg_analog_flag as is_analog FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN international_name i WITH(NOLOCK) ON i.international_name_id = mpn.international_name_id INNER JOIN pharmacological_group f WITH(NOLOCK) ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.mpn_PriceOff_Flag = 0 ORDER BY name";
            sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            //SqlDataAdapter mpnData = new SqlDataAdapter(sc);
            
            data = sc.ExecuteReader();
            try
            {
                double val = 0;
                int loadingInterval = 0;
                Dispatcher.Invoke(() => { MpnBar.Maximum = rowCount; });

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
                    Dispatcher.Invoke(() => { 
                        MpnList.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog));                            
                    });
                    loadingInterval++;
                    val++;
                    if (CPUCores > 1 && (loadingInterval == SearchMainWindow.LoadingStepBig || val == rowCount ))
                    {
                        Dispatcher.Invoke(() => {
                            MpnLoadingText.Text = String.Format("{0}/{1}", val, rowCount);
                            MpnBar.Value = val;
                        });
                        //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, val });
                        loadingInterval = 0;
                    }
                }
                //mpnData.Fill(MPN_DT);
            }
            finally
            {
                mpns = new ObservableCollection<MPNClass>(MpnList);
                Dispatcher.Invoke(() =>
                {
                    MPN.ItemsSource = mpns;
                    MPNList.ItemsSource = mpns;
                    if (CPUCores < 2)
                    {
                        MpnBar.Value = MpnBar.Maximum;
                        MpnBar.IsIndeterminate = false;
                    }
                });
                /*Dispatcher.Invoke(() =>
                {
                    MPN.ItemsSource = CommonData.Tables["MPN"].DefaultView;
                    MPNList.ItemsSource = CommonData.Tables["MPN"].DefaultView;
                });*/
                data.Close();
                ch_d_1_dbc.Close();
                
            }
        }
        private void PreloadACInter()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(InterBar.SetValue);

            List<InterClass> IntList = new List<InterClass>();

            string sqlCount = "SELECT COUNT(*) as count FROM international_name WITH (NOLOCK) WHERE in_sinonim_flag = 1 ";
            int rowCount = 0;
            SqlCommand sc = new SqlCommand(sqlCount, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            data.Read();
            rowCount = Convert.ToInt32( data["count"].ToString() );
            data.Close();
            ch_d_1_dbc.Close();
            
            string sql = "SELECT international_name_name as iname, international_name_id as id FROM international_name WITH (NOLOCK) WHERE in_sinonim_flag = 1 ORDER BY international_name_name ASC";
            sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            data = sc.ExecuteReader();

            try
            {
                double val = 0;
                Dispatcher.Invoke(() => { InterBar.Maximum = rowCount; });
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["iname"].ToString();
                    Dispatcher.Invoke(() => { 
                        IntList.Add(new InterClass(id, name, true));
                        if (CPUCores > 1)
                            InterLoadingText.Text = String.Format("{0}/{1}", val + 1, rowCount); 
                    });
                    if (CPUCores > 1)
                        Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++val });
                }
            }
            finally
            {
                inters = new ObservableCollection<InterClass>(IntList);
                Dispatcher.Invoke(() =>
                {
                    Inter.ItemsSource = inters;
                    InterList.ItemsSource = inters;
                    if (CPUCores < 2)
                    {
                        InterBar.Value = InterBar.Maximum;
                        InterBar.IsIndeterminate = false;
                    }
                });
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACFarm()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(FarmBar.SetValue);

            List<FarmClass> FarList = new List<FarmClass>();

            string sqlCount = "SELECT COUNT(*) as count FROM pharmacological_group a WITH (NOLOCK) WHERE a.pg_analog_flag = 1 ";
            int rowCount = 0;
            SqlCommand sc = new SqlCommand(sqlCount, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            data.Read();
            
            rowCount = Convert.ToInt32(data["count"].ToString());
            data.Close();
            ch_d_1_dbc.Close();


            string sql = "SELECT a.pharmacological_group_id as id, a.pharmacological_group_name as fname FROM pharmacological_group a WITH (NOLOCK) WHERE a.pg_analog_flag = 1 ORDER BY a.pharmacological_group_name ASC";
            sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            data = sc.ExecuteReader();

            try
            {
                double val = 0;
                Dispatcher.Invoke(() => { FarmBar.Maximum = rowCount; });
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["fname"].ToString();
                    Dispatcher.Invoke(() => { 
                        FarList.Add(new FarmClass(id, name, true));
                        if (CPUCores > 1)
                            FarmLoadingText.Text = String.Format("{0}/{1}", val + 1, rowCount); 
                    });
                    if (CPUCores > 1)
                        Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++val });
                }
            }
            finally
            {
                farms = new ObservableCollection<FarmClass>(FarList);
                Dispatcher.Invoke(() =>
                {
                    Farm.ItemsSource = farms;
                    FarmList.ItemsSource = farms;
                    if (CPUCores < 2)
                    {
                        FarmBar.Value = FarmBar.Maximum;
                        FarmBar.IsIndeterminate = false;
                    }
                });
                data.Close();
                ch_d_1_dbc.Close();
            }
        }
        private void PreloadACCity()
        {
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(CityBar.SetValue);

            List<CityClass> CityList = new List<CityClass>();

            string sqlCount = "SELECT COUNT(*) as count FROM area a WITH (NOLOCK) WHERE a.area_SortCode > 0 ";
            int rowCount = 0;
            SqlCommand sc = new SqlCommand(sqlCount, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            data.Read();
            rowCount = Convert.ToInt32(data["count"].ToString());
            data.Close();
            ch_d_1_dbc.Close();

            string sql = "SELECT a.area_name as cname, a.area_id as id FROM area a WITH (NOLOCK) WHERE a.area_SortCode > 0 ORDER BY a.area_SortCode ASC";
            sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            data = sc.ExecuteReader();

            try
            {
                double val = 0;
                Dispatcher.Invoke(() => { CityBar.Maximum = rowCount; });
                while (data.Read())
                {
                    int id = Convert.ToInt32(data["id"].ToString());
                    string name = data["cname"].ToString();
                    Dispatcher.Invoke(() => { 
                        CityList.Add(new CityClass(id, name));
                        if (CPUCores > 1)
                            CityLoadingText.Text = String.Format("{0}/{1}", val + 1, rowCount); 
                    });
                    if (CPUCores > 1)
                        Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++val });
                }
            }
            finally
            {
                cities = new ObservableCollection<CityClass>(CityList);
                Dispatcher.Invoke(() => { 
                    City.ItemsSource = cities;
                    if (CPUCores < 2)
                    {
                        CityBar.IsIndeterminate = false;
                        CityBar.Value = CityBar.Maximum;
                    }
                });
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
        private void ChangeInterFarmComboBox(MPNClass source)
        {
            if (source != null)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Inter.Text = source.GetInterName();
                    Farm.Text = source.GetFarmName();
                    MPN.Text = source.ToString();
                });
                
            }
        }

        private void MPN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass sel_elem = (MPNClass)MPN.SelectedItem;
            if (sel_elem != null)
                FLog.Log(String.Format("MPN_SelectionChanged. MPN изменен на {0}({1})", sel_elem.ToString(), sel_elem.GetID()));
        }
        
        private void Inter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InterClass SelElem = (InterClass)Inter.SelectedItem;
            if(SelElem != null)
                FLog.Log(String.Format("Inter_SelectionChanged. Inter изменен на {0}({1})", SelElem.ToString(), SelElem.GetID()));
        }

        private void Farm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FarmClass SelElem = (FarmClass)Farm.SelectedItem;
            if (SelElem != null)
                FLog.Log(String.Format("Farm_SelectionChanged. Farm изменен на {0}({1})", SelElem.ToString(), SelElem.GetID()));
        }

        private void MPN_KeyUp(object sender, KeyEventArgs e)
        {
            MPNClass sel_elem = (MPNClass)MPN.SelectedItem;
            if (e.Key == Key.Return && sel_elem != null)
            {
                DebugText.Text += "\r\nНажата клавиша Enter " + String.Format("Выбрано- {0}.{1}", sel_elem.GetID(), sel_elem.ToString());
                FLog.Log(String.Format("MPN_KeyUp. Нажата клавиша Enter. Выбрано- {0}.{1}", sel_elem.GetID(), sel_elem.ToString()));
                
                int index = MPN.SelectedIndex;
                MPNList.SelectedIndex = index;
                MPNList.ScrollIntoView(MPNList.SelectedItem);
                
                ChangeInterFarmComboBox(sel_elem);
            }
        }
        private void Inter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                InterClass SelElem = (InterClass)Inter.SelectedItem;
                if (SelElem != null)
                {
                    DebugText.Text += "\r\nНажата клавиша Enter " + String.Format("Выбрано- {0}.{1}", SelElem.GetID(), SelElem.ToString());
                    FLog.Log(String.Format("Inter_KeyUp. Нажата клавиша Enter. Выбрано- {0}.{1}", SelElem.GetID(), SelElem.ToString()));
                    int index = Inter.SelectedIndex;
                    InterList.SelectedIndex = index;
                    InterList.ScrollIntoView(InterList.SelectedItem);
                }
            }
        }
        private void Farm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FarmClass SelElem = (FarmClass)Farm.SelectedItem;
                if (SelElem != null)
                {
                    DebugText.Text += "\r\nНажата клавиша Enter " + String.Format("Выбрано- {0}.{1}", SelElem.GetID(), SelElem.ToString());
                    FLog.Log(String.Format("Farm_KeyUp. Нажата клавиша Enter. Выбрано- {0}.{1}", SelElem.GetID(), SelElem.ToString()));
                    int index = Farm.SelectedIndex;
                    FarmList.SelectedIndex = index;
                    FarmList.ScrollIntoView(FarmList.SelectedItem);
                }
            }
        }
        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CityClass selectedCity = (CityClass)City.SelectedItem;
            if(selectedCity != null)
                FLog.Log(String.Format("City_SelectionChanged. Город изменен на {0}({1})", selectedCity.ToString(), selectedCity.GetID()));
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
            /*if (e.Key == Key.Return)
            {
                CityClass selectedCity = (CityClass)City.SelectedItem;
                if (selectedCity != null)
                {
                    DebugText.Text += "\r\nНажата клавиша Enter " + String.Format("Выбрано- {0}.{1}", selectedCity.GetID(), selectedCity.ToString());
                    Address.IsEnabled = true;
                    PreloadACComplex();
                }
                else
                {
                    Address.IsEnabled = false;
                    complexies.Clear();
                }
            }*/
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
        private void FilterLocalTextBoxWork()
        {
            if (filters != null)
                filters.Clear();

            string FilterText = Filter.Text;
            DebugText.Text += String.Format("\r\nтекст фильтра - \"{0}\"", FilterText);
            DebugText.ScrollToEnd();
            FLog.Log(String.Format("FilterLocalTextBoxWork. текст фильтра - \"{0}\"", FilterText));
            List<MPNClass> FullContains = new List<MPNClass>();
            List<MPNClass> AllSliceContains = new List<MPNClass>();
            List<MPNClass> SliceContains = new List<MPNClass>();
            if (FilterText != "" && FilterText != null && FilterText.Length > 2)
            {
                string[] words;
                words = FilterText.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
                int CollectionCount = mpns.Count;
                if (mpns.Count > 0)
                {
                    Parallel.For(0, CollectionCount, (i) =>
                    {
                        int contains = 0;
                        string Text = mpns[i].ToString().ToLower();
                        if (Text.Contains(FilterText.ToLower()))
                        {
                            FullContains.Add(mpns[i]);
                        }
                        else
                        {
                            foreach (string word in words)
                            {
                                if (word.Length > 2 && Text.StartsWith(word.ToLower()))
                                {
                                    SliceContains.Add(mpns[i]);
                                }
                                if (word.Length > 2 && Text.Contains(word.ToLower()))
                                    contains = contains + 1;
                            }
                            if (words.Count<string>() > 1 && contains > 0 && words.Count<string>() == contains)
                            {
                                AllSliceContains.Add(mpns[i]);
                                contains = 0;
                            }
                        }
                    });
                    //for (int i = 0; i < CollectionCount; i++)
                    //{
                    //    string Text = mpns[i].ToString().ToLower();
                    //    if (Text.Contains(FilterText.ToLower()))
                    //    {
                    //        FullContains.Add(mpns[i]);
                    //        //filters.Add(mpns[i]);
                    //    }
                    //    else
                    //    {
                    //        foreach (string word in words)
                    //        {
                    //            if(word.Length > 2 && Text.Contains( word.ToLower() ) )
                    //            {
                    //                SliceContains.Add(mpns[i]);
                    //                //filters.Add(mpns[i]);
                    //            }
                    //        }
                    //    }
                    //}
                    Dispatcher.Invoke(() =>
                    {
                        DebugText.Text += String.Format("\r\nПолное совпадение: {0}, Начинающееся с одного из слов: {1}, Есть все слова: {2}", FullContains.Count, SliceContains.Count, AllSliceContains.Count);
                        DebugText.ScrollToEnd();
                        FLog.Log(String.Format("Полное совпадение: {0}, Начинающееся с одного из слов: {1}, Есть все слова: {2}", FullContains.Count, SliceContains.Count, AllSliceContains.Count));
                        filters = new ObservableCollection<MPNClass>(FullContains.Concat(AllSliceContains).ToList<MPNClass>().Concat(SliceContains).ToList<MPNClass>());
                        FilterList.ItemsSource = filters;
                    });
                }
                //FilterList.SelectedIndex = -1;
            }
        }
        private void FilterTextBoxWork()
        {
            filters.Clear();
            string FilterText = Filter.Text;
            DebugText.Text += String.Format("\r\nтекст фильтра - \"{0}\"", FilterText);
            DebugText.ScrollToEnd();
            FLog.Log(String.Format("FilterTextBoxWork. текст фильтра - \"{0}\"", FilterText));
            if (FilterText != "" && FilterText != null && FilterText.Length > 2)
            {
                SqlConnection ch_php = new SqlConnection(Properties.Settings.Default.ch_phpConnectionString);
                string sql = "declare @translate_lang_id int = 1 -- язык поиска \r\ndeclare @str varchar(250) = @fstr \r\ndeclare @top int = 100 -- количество возвращаемых записей \r\ndeclare @search_type int = 1 --0 ищем по xxx%, 1 ищем сначала по xxx%, а затем по %xxx% \r\ndeclare @t table( r_num int, medical_product_name_id int, medical_product_name_name varchar(250), step int, word_cnt int, error_level int, word_num int, medical_product_name_stat int ) \r\ninsert into @t exec ch_php_code.dbo.p_search_mpn_by_str @translate_lang_id, @str, @search_type, @top \r\nSELECT DISTINCT mpn.medical_product_name_name as name, t.r_num, mpn.medical_product_name_id as id, i.international_name_name as inter_name,	i.international_name_id as inter_id, f.pharmacological_group_id as farm_id,	f.pharmacological_group_name as farm_name, 1 as is_sinonim, 1 as is_analog FROM medical_product_name mpn WITH (NOLOCK) INNER JOIN @t t ON mpn.medical_product_name_id = t.medical_product_name_id LEFT JOIN international_name i ON i.international_name_id = mpn.international_name_id LEFT JOIN pharmacological_group f ON f.pharmacological_group_id = mpn.pharmacological_group_id WHERE mpn.translate_lang_id = @translate_lang_id AND (f.translate_lang_id = @translate_lang_id OR f.pharmacological_group_name IS NULL ) AND (i.translate_lang_id = @translate_lang_id OR i.international_name_name IS NULL) order by t.r_num";
                SqlCommand sc = new SqlCommand(sql, ch_php);
                sc.Parameters.AddWithValue("@fstr", FilterText);
                ch_php.Open();
                SqlDataReader data = sc.ExecuteReader();

                try
                {
                    while (data.Read())
                    {
                        string interName = "~";
                        int interId = 456;
                        int farmId = 309;
                        string farmName = "~";



                        int id = Convert.ToInt32(data["id"].ToString());
                        string name = data["name"].ToString();
                        if (data["inter_name"].ToString() != null && data["inter_name"].ToString() != "")
                            interName = data["inter_name"].ToString();
                        if (data["inter_id"].ToString() != null && data["inter_id"].ToString() != "")
                            interId = Convert.ToInt32(data["inter_id"].ToString());
                        if (data["farm_id"].ToString() != null && data["farm_id"].ToString() != "")
                            farmId = Convert.ToInt32(data["farm_id"].ToString());
                        if (data["farm_name"].ToString() != null && data["farm_name"].ToString() != "")
                            farmName = data["farm_name"].ToString();
                        bool isSinonim = Convert.ToBoolean(Convert.ToInt32(data["is_sinonim"].ToString()));
                        bool isAnalog = Convert.ToBoolean(Convert.ToInt32(data["is_analog"].ToString()));
                        Dispatcher.BeginInvoke(new ThreadStart(delegate { filters.Add(new MPNClass(id, name, interId, interName, farmId, farmName, isSinonim, isAnalog)); }));
                    }
                }
                /*catch (Exception excp)
                {
                    MessageBox.Show(String.Format("{0}\r\n{1}", excp.Message, excp.Source));
                }*/
                finally
                {
                    ch_php.Close();
                    data.Close();
                }
            }
        }
        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DebugMode(
                    true,
                    () =>
                    {
                        if (LocalCheck.IsChecked == true)
                            FilterLocalTextBoxWork();
                        else
                            FilterTextBoxWork();
                    },
                    "Формирование листинга фильтра");
            }
        }
        private void FilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filters.Count > 0)
            {
                MPNClass SelItem = (MPNClass)FilterList.SelectedItem;
                if (SelItem != null)
                {
                    Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { if (LocalCheck.IsChecked == true) LocalSinonimsAnalogsAction(SelItem); else SinonimsAnalogsAction(SelItem); }, "Выбор в листинге фильтра"); }));
                    SelThread.SetApartmentState(ApartmentState.STA);
                    SelThread.Start();
                    ChangeInterFarmComboBox(SelItem);
                }
            }
        }
        private void MPNList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass SelItem = (MPNClass)MPNList.SelectedItem;
            if (SelItem != null)
            {
                Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { if (LocalCheck.IsChecked == true) LocalSinonimsAnalogsAction(SelItem); else SinonimsAnalogsAction(SelItem); }, "Листинг Наименований"); }));
                SelThread.SetApartmentState(ApartmentState.STA);
                SelThread.Start();
                ChangeInterFarmComboBox(SelItem);
            }
        }
        private void InterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InterClass SelItem = (InterClass)InterList.SelectedItem;
            if (SelItem != null)
            {
                Thread SelThread = new Thread(
                    new ThreadStart(
                        delegate
                        {
                            DebugMode(() => { if (LocalCheck.IsChecked == true) LocalSinonimAction(SelItem); else SinonimsAction(SelItem); }, "Листинг Междунар.Наим. " + SelItem.IsSinonim().ToString());
                        }
                        )
                        );
                SelThread.SetApartmentState(ApartmentState.STA);
                SelThread.Start();
            }
        }
        private void FarmList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FarmClass SelItem = (FarmClass)FarmList.SelectedItem;
            if (SelItem != null)
            {
                Thread SelThread = new Thread(new ThreadStart(delegate { DebugMode(() => { if (LocalCheck.IsChecked == true) LocalAnalogsAction(SelItem); else AnalogsAction(SelItem); }, "Листинг Фарм.Аналогов " + SelItem.IsAnalog().ToString()); }));
                SelThread.SetApartmentState(ApartmentState.STA);
                SelThread.Start();
            }
        }
        private void SelectedListChangeItem(object sender, NotifyCollectionChangedEventArgs arg)
        {
            DebugText.Text += String.Format("\r\n Изменен список выбранных продуктов");
            FLog.Log(String.Format("SelectedListChangeItem. Изменен список выбранных продуктов"));
            NewFormaTab(arg);
        }
        private void Sinonim_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass SelItem = (MPNClass)Sinonim.SelectedItem;
            if (SelItem != null)
            {
                ChangeInterFarmComboBox(SelItem);
            }
        }
        private void Analog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MPNClass SelItem = (MPNClass)Analog.SelectedItem;
            if (SelItem != null)
            {
                ChangeInterFarmComboBox(SelItem);
            }
        }
        #endregion

        #region local actions for list events
        private void LocalSinonimAction(InterClass SelItem)
        {
            if (SelItem != null && SelItem.IsSinonim())
            {
                if (inters.Count > 0)
                {
                    sinonims.Clear();
                    foreach (MPNClass item in mpns)
                    {
                        if (item.GetInterID() == SelItem.GetID() && item.IsSinonim() == true)
                        {
                            sinonims.Add(item);
                        }
                    }
                }
            }
        }
        private void LocalAnalogsAction(FarmClass SelItem)
        {
            if (SelItem != null && SelItem.IsAnalog())
            {
                if (farms.Count > 0)
                {
                    analogs.Clear();
                    foreach (MPNClass item in mpns)
                    {
                        if (item.GetFarmID() == SelItem.GetID() && item.IsAnalog() == true)
                        {
                            analogs.Add(item);
                        }
                    }
                }
            }
        }
        public void LocalSinonimsAnalogsAction(MPNClass SelItem)
        {
            if (SelItem != null)
            {
                int InterId = SelItem.GetInterID();
                int FarmId = SelItem.GetFarmID();
                if (inters.Count > 0 && farms.Count > 0)
                {
                    sinonims.Clear();
                    analogs.Clear();
                    foreach (MPNClass item in mpns)
                    {
                        if (item.GetInterID() == InterId && SelItem.IsSinonim())
                        {
                            sinonims.Add(item);
                        }
                        if (item.GetFarmID() == FarmId && SelItem.IsAnalog())
                        {
                            analogs.Add(item);
                        }
                    }
                }
                ChangeInterFarmComboBox(SelItem);
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
            DebugText.Text += String.Format("\r\nsinonim = \"{0}\", inter = \"{1}\"", sql_sinonim, InterId);
            DebugText.Text += String.Format("\r\nanalog = \"{0}\", pharm = \"{1}\"", sql_farm_analog, FarmId);
            DebugText.ScrollToEnd();
            FLog.Log(String.Format("SinonimsAnalogsAction. sinonim = \"{0}\", inter = \"{1}\"", sql_sinonim, InterId));
            FLog.Log(String.Format("SinonimsAnalogsAction. analog = \"{0}\", pharm = \"{1}\"", sql_farm_analog, FarmId));
            if (SelItem.IsSinonim())
            {
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
            }
            if (SelItem.IsAnalog())
            {
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
        }
        public void SinonimsAction(InterClass Inter)
        {
            if (Inter != null && Inter.IsSinonim())
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
        }
        public void AnalogsAction(FarmClass Pharm)
        {
            if (Pharm != null && Pharm.IsAnalog())
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
        }
        #endregion

        #region Buttons
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
            FLog.Log(String.Format("MainTabs_SelectionChanged. Выбранная вкладка - {0}", MainTabs.SelectedIndex));
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

        private void MPNList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)MPNList.SelectedItem;
            if (selectedMpns.Count != 0)
            {
                int TabIndex = Convert.ToInt32(selectedMpns[selectedMpns.Count - 1].Index) + 1;
                selectMpn.Index = TabIndex;
            }
            else
            {
                if (selectMpn != null)
                    selectMpn.Index = 0;
                else
                    return;
            }
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void FilterList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MPNClass selectMpn = (MPNClass)FilterList.SelectedItem;
            if (selectedMpns.Count != 0)
            {
                int TabIndex = Convert.ToInt32(selectedMpns[selectedMpns.Count - 1].Index) + 1;
                selectMpn.Index = TabIndex;
            }
            else
            {
                selectMpn.Index = 0;
            }
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void Sinonim_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            MPNClass selectMpn = (MPNClass)Sinonim.SelectedItem;
            if (selectedMpns.Count != 0)
            {
                int TabIndex = Convert.ToInt32(selectedMpns[selectedMpns.Count - 1].Index) + 1;
                selectMpn.Index = TabIndex;
            }
            else
            {
                if (selectMpn != null)
                    selectMpn.Index = 0;
                else
                    return;
            }
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }

        private void Analog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            MPNClass selectMpn = (MPNClass)Analog.SelectedItem;
            if (selectedMpns.Count != 0)
            {
                int TabIndex = Convert.ToInt32(selectedMpns[selectedMpns.Count - 1].Index) + 1;
                selectMpn.Index = TabIndex;
            }
            else
            {
                if (selectMpn != null)
                    selectMpn.Index = 0;
                else
                    return;
            }
            selectedMpns.Add(selectMpn);
            SelectedMPNSwitcher.Content = String.Format("Выбранные позиции ({0})", selectedMpns.Count);
        }
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            MPN.SelectedIndex = -1;
            Inter.SelectedIndex = -1;
            Farm.SelectedIndex = -1;
            MPNList.SelectedIndex = -1;
            InterList.SelectedIndex = -1;
            FarmList.SelectedIndex = -1;
            Address.SelectedIndex = -1;
            City.SelectedIndex = -1;
            MainTabs.SelectedIndex = ListPage;
            SelectedMPNSwitcher.Content = "Выбранные позиции (0)";
            Filter.Text = "";

            formas.Clear();
            mps.Clear();
            selectedMpns.Clear();
            sinonims.Clear();
            analogs.Clear();
            if (filters != null)
                filters.Clear();
            FLog.Log("ClearAll_Click. Форма очищена");
        }

        private void AddMpn_Click(object sender, RoutedEventArgs e)
        {

            MainTabs.SelectedIndex = ListPage;
            MPN.SelectedIndex = -1;
            Inter.SelectedIndex = -1;
            Farm.SelectedIndex = -1;
            MPNList.SelectedIndex = -1;
            InterList.SelectedIndex = -1;
            FarmList.SelectedIndex = -1;
            //Address.SelectedIndex = -1;
            sinonims.Clear();
            analogs.Clear();
            FLog.Log("AddMpn_Click. Нажата кнопка добавления препарата");
        }
        #endregion

        #region wpf controls work

        private void SearchWindow_Closed(object sender, EventArgs e)
        {
            FLog.CloseLogStream();
        }

        private void LocalCheck_Checked(object sender, RoutedEventArgs e)
        {
            FLog.Log("Установлен локальный режим");
        }

        private void LocalCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FLog.Log("Выключен локальный режим");
        }

        private void NewFormaTab(NotifyCollectionChangedEventArgs arg)
        {
            if (arg != null && arg.NewItems != null)
            {
                TabItem NewTab = new TabItem();
                MPNClass newMpn = (MPNClass)arg.NewItems[0];
                NewTab.Header = String.Format("{0}", newMpn.ToString());
                NewTab.Tag = newMpn.Index;
                FormaTabs.Items.Add(NewTab);
                NewTab.IsSelected = true;
                MainTabs.SelectedIndex = FormaPage;
                Dispatcher.BeginInvoke(new ThreadStart(delegate { GenerateFormaForm(newMpn, NewTab); }));
            }
            else
            {
                FormaTabs.Items.Clear();
            }
            
        }
        private void GenerateFormaForm(MPNClass MpnForma, TabItem TI)
        {            
            UserControl UC = new SecondPage(this);
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
                GenerateFormaSources(args);
                LV = (ListView)UC.FindName("MpGrid");
                args.ControlObject = LV;
                GenerateMpSources(args);
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
        
        private void GenerateFormaSources(FormEventArgs e)
        {
            MPNClass mpn = (MPNClass)e.DataObject;
            ListView LV = (ListView)e.ControlObject;
            int index = FormaTabs.Items.Count - 1;
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
                    FormaClass formaObj = new FormaClass(id, MpnId, FormaName, true);
                    int ind = formaSource.Count;
                    formaObj.Index = ind;
                    formaSource.Add(formaObj);
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
        private void GenerateMpSources(FormEventArgs e)
        {
            MPNClass mpn = (MPNClass)e.DataObject;
            ListView LV = (ListView)e.ControlObject;
            int index = FormaTabs.Items.Count - 1;
            ObservableCollection<MpClass> mpSource = new ObservableCollection<MpClass>();
            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            string sql = "SELECT mpn.medical_product_name_name as mpn, mpn.medical_product_name_id as mpn_id, mf.medical_form_name as mfn, mp.medical_product_id as mp_id, mp.medical_product_str as mp_str1, mp.medical_product_str2 as mp_str2, mp.medical_form_id as mf_id FROM medical_product mp WITH (NOLOCK) INNER JOIN medical_product_name mpn WITH (NOLOCK) ON mp.medical_product_name_id = mpn.medical_product_name_id INNER JOIN medical_form mf WITH (NOLOCK) ON mp.medical_form_id = mf.medical_form_id WHERE mp.medical_product_name_id = @mpn_id ORDER BY mpn.medical_product_name_name ASC, mp.medical_form_id ASC, mp.medical_product_str ASC";
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            sc.Parameters.AddWithValue("@mpn_id", mpn.GetID());
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
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
                mps.Add(index, mpSource); //Добавляем источник форм в общий словарь источников форм
            }
            finally
            {
                LV.Items.Clear();
                LV.ItemsSource = mps[index]; //Присоединяем источник к списку из общего словаря*/

                ch_d_1_dbc.Dispose();
                data.Close();
            }
        }

        private void SelectedMPN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainTabs.SelectedIndex = FormaPage;
            int Index = SelectedMPN.SelectedIndex;
            MPNClass SelectedItem = (MPNClass)SelectedMPN.SelectedItem;
            int index = SelectedItem.Index;
            foreach(object TI in FormaTabs.Items)
            {
                if (TI.GetType() == typeof(TabItem))
                {
                    TabItem Tab = (TabItem)TI;
                    int tag = Convert.ToInt32(Tab.Tag.ToString());
                    if (tag == index)
                    {
                        Tab.IsSelected = true;
                        SelectedMPN.Visibility = Visibility.Hidden;
                    }
                }
            }
            FLog.Log(String.Format("SelectedMPN_SelectionChanged. Выбор препарата \"{0}\" в списке выбранных.", SelectedItem.ToString()));
        }

        public void SetFirmId(int Firm)
        {
            FirmId = Firm;
            FLog.Log(String.Format("SetFirmId. Установлена фирма {0}.", Firm));
        }
        public void SetUserId(int User)
        {
            UserId = User;
            FLog.Log(String.Format("SetUserId. Установлен пользователь {0}.", User));
            FLog.AddUser(User); //Добавляем id пользователя для записи в лог
        }

        public void AutoAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            //Сбор данных и инициализация события автоответа
            List<int> DrugstoreIds = new List<int>();
            foreach (DrugstoreInfo final in finals)
            {
                if (final.Selected == true)
                {
                    if (DrugstoreIds.IndexOf(final.DDId) < 0)
                    {
                        DrugstoreIds.Add(final.DDId);
                    }
                }
            }
            if(onAutoAnswer != null)
                onAutoAnswer(DrugstoreIds);

            FLog.Log(String.Format("Обработано событие автоответа. переданные ID аптек: {0}", String.Join(",", DrugstoreIds)));
            var result = MessageBox.Show("Очистить форму?", "Автоответ успешно отправлен", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                this.ClearAll_Click(ClearAll, new RoutedEventArgs());
            }
        }

        //Тестовая реализация обработчика события автоответа
        public void testAutoAnswerMessage(List<int> Ids)
        {
            return;
        }

        public bool SetCityFromCall(int cityId)
        {
            if (cityId > 0)
            {
                FLog.Log(String.Format("SetCityFromCall. Автоматически установлен город {0}", cityId));
                int index = 0;
                foreach(CityClass city in cities)
                {
                    if (cityId == city.GetID())
                    {
                        City.SelectedIndex = index;
                        return true;
                    }
                    index++;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if(SetCityFromCall(23))
                DebugText.Text += String.Format("\r\nСимуляция выбора города по звонку. Успешно!");
            else
                DebugText.Text += String.Format("\r\nСимуляция выбора города по звонку. Не удалось!");
        }

    }

    
}
