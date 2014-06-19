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
using System.Data;
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
        string[] Sort = new string[]
        {
                    "find_key desc",
                    "search_Count desc",
                    "ds_complex_priority asc",
                    "dd_count asc",
                    "ds_name",
                    "dd_name",
                    "mpn_name",
                    "mf_name",
                    "p_name",
                    "mp_str"
        };
        public int FormaRowIndex    = 0;
        public int MpRowIndex       = 0;

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

        private void FormaGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int TabIndex = Main.FormaTabs.SelectedIndex;

            ListBoxItem obj = sender as ListBoxItem;
            if (obj.Content.GetType() == typeof( FormaClass ) )
            {
                FormaClass FaC = (FormaClass)obj.Content;
                if (e.MouseDevice.Target.GetType() == typeof(TextBlock))
                {
                    TextBlock Target = (TextBlock)e.MouseDevice.Target;
                    string Value = (string)FaC.GetValueByName(Target.Name);
                    foreach (FormaClass fc in Main.formas[TabIndex])
                    {
                        if (fc.FormaName == Value)
                            fc.Selected = true;
                        else
                            fc.Selected = false;
                    }
                    FormaGrid.Items.Refresh();
                }
            }
            else
            {
                MpClass MpC = (MpClass)obj.Content;
                if (e.MouseDevice.Target.GetType() == typeof(TextBlock))
                {
                    TextBlock Target = (TextBlock)e.MouseDevice.Target;
                    string Value = (string)MpC.GetValueByName(Target.Name);
                    int SelIndex = MpC.Index;
                    string str = String.Format("{0}", Target.Name).ToLower();
                    switch (str)
                    {
                        case "mpname":
                            foreach (MpClass mpcl in Main.mps[TabIndex])
                            {
                                if (mpcl.MpName == Value)
                                    mpcl.Selected = true;
                                else
                                    mpcl.Selected = false;
                            }
                            break;
                        case "formaname":
                            foreach (MpClass mpcl in Main.mps[TabIndex])
                            {
                                if (mpcl.MpName == MpC.MpName)
                                {
                                    if (mpcl.FormaName == Value)
                                        mpcl.Selected = true;
                                    else
                                        mpcl.Selected = false;
                                }
                                else
                                    mpcl.Selected = false;
                            }
                            break;
                        case "op1":
                            foreach (MpClass mpcl in Main.mps[TabIndex])
                            {
                                if (mpcl.MpName == MpC.MpName)
                                {
                                    if (mpcl.FormaName == MpC.FormaName)
                                    {
                                        if (mpcl.Description1 == Value)
                                            mpcl.Selected = true;
                                        else
                                            mpcl.Selected = false;
                                    }
                                    else
                                        mpcl.Selected = false;
                                }
                                else
                                    mpcl.Selected = false;
                            }
                            break;
                        case "op2":
                            foreach (MpClass mpcl in Main.mps[TabIndex])
                            {
                                if (mpcl.MpName == MpC.MpName)
                                {
                                    if (mpcl.FormaName == MpC.FormaName)
                                    {
                                        if (mpcl.Description1 == MpC.Description1)
                                        {
                                            if(mpcl.Description2 == Value)
                                                mpcl.Selected = true;
                                            else
                                                mpcl.Selected = false;
                                        }
                                        else
                                            mpcl.Selected = false;
                                    }
                                    else
                                        mpcl.Selected = false;
                                }
                                else
                                    mpcl.Selected = false;
                            }
                            break;
                    }
                    MpGrid.Items.Refresh();
                }
            }
            
            //TextBlock tb = (TextBlock)sender;
            //int RowIndex = Convert.ToInt32(tb.Tag.ToString());
            //int TabIndex = Main.FormaTabs.SelectedIndex;
            //ObservableCollection<FormaClass> Source = Main.formas[TabIndex];
            //string SearchFormaName = Source[RowIndex].FormaName;
            //foreach(FormaClass fc in Source)
            //{
            //    if (fc.FormaName == SearchFormaName)
            //    { Main.formas[TabIndex][RowIndex].Selected = true; }
            //    else
            //    { Main.formas[TabIndex][RowIndex].Selected = false; }
            //}
            //FormaGrid.ItemsSource = Main.formas[TabIndex];
            //FormaGrid.Items.Refresh();
        }

        private void DdSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Main.City.SelectedItem != null && Main.Address.SelectedItem != null)
            {
                FinalPage Final = new FinalPage(this.Main);
                Main.FinalTab.Content = Final;
                string genericSql = GenerateMpSqlTable();
                if (GetFinalInfo(genericSql, 0))
                {
                    Final.FinalGrid.ItemsSource = Main.finals;
                    Main.MainTabs.SelectedIndex = MainWindow.FinalPage;
                }
                else
                {
                    MessageBox.Show("Результаты по выбранному городу не найдены. Попробуйте найти во всех городах.");
                    Main.MainTabs.SelectedIndex = MainWindow.FormaPage;
                }
                
            }
            else
            {
                MessageBox.Show("Не выбран адрес клиента!");
                Main.Address.Focus();
            }
        }

        private void DdAllSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Main.City.SelectedItem != null && Main.Address.SelectedItem != null)
            {
                FinalPage Final = new FinalPage(this.Main);
                Main.FinalTab.Content = Final;
                string genericSql = GenerateMpSqlTable();
                if (GetFinalInfo(genericSql, 1))
                {
                    Final.FinalGrid.ItemsSource = Main.finals;
                    Main.MainTabs.SelectedIndex = MainWindow.FinalPage;
                }
                else
                {
                    MessageBox.Show("Результаты по такому запросу в базе не найдены во всех городах. Поменяйте запрос и попробуйте снова");
                    Main.MainTabs.SelectedIndex = MainWindow.FormaPage;
                }
                
            }
            else
            {
                MessageBox.Show("Не выбран адрес клиента!");
                Main.Address.Focus();
            }
        }

        /*
         *TODO Передача в функцию ID фирмы (от управляющей програмы) 
         */
        protected bool GetFinalInfo(string predSql, int all_area)
        {
            int firm_id = Main.FirmId ; //Костыль!! Надо принимать валидный id фирмы от управляющей программы
            int user_id = Main.UserId; //Костыль!! Надо принимать правильный user_id от управляющей программы
            
            ComplexClass SelComplex = (ComplexClass)Main.Address.SelectedItem;
            int complex_id = SelComplex.GetID();
            string sql = String.Format(@"declare @block0 varchar(250) = convert(varchar, getdate(), 126);
                declare @area_all int = {0};
                declare @t_param table (  page_num int, medical_product_name_id int, medical_form_id int, packing_id int, country_id int, medical_product_id int, medical_product2_id int) insert into @t_param select * from ({1}) t
                declare @firm_id int = 0;  declare @c_id int; set @c_id = {2};
                declare @ds_user_id int; set @ds_user_id = {3};
                declare @stat_query_tel varchar(250); set @stat_query_tel = '003';
                declare @dd_cnt table (drugstore_department_id int, cnt int)
                                    insert into @dd_cnt exec('SELECT
                                      drugstore_department_id,
                                      COUNT(*) cnt
                                    FROM
                                      ch_php.dbo.stat_report_answer_reestr with(nolock)
                                    WHERE stat_inet_tsdate > GETDATE()-30 and
                                      stat_inet_type = 2
                                    GROUP BY
                                      drugstore_department_id
                                    order by  cnt desc') at isrv

                ----------------------------------------------

                declare @area_id int;
                set @area_id = (select area_id from complex with(nolock) where complex_id = @c_id)

                -----------------------------------------------




                declare @block2 varchar(250) = convert(varchar, getdate(), 126)

                declare @t_dd table (drugstore_department_id int, rnd int)

                insert into @t_dd
                select
                  drugstore_department_id,
                  1 as rnd
                from
                  drugstore_department dd with(nolock)
                    inner join complex c with(nolock) on dd.complex_id = c.complex_id
                where
                  (c.area_id = @area_id or @area_all = 1) and (dd.firm_id=@firm_id or @firm_id=0)
                ", all_area, predSql, complex_id,user_id);
                if(firm_id != null && firm_id > 0)
                    sql += " and dd.find_key > 0";

               sql += String.Format(@"
               ------------------------------------------------------

                declare @block3 varchar(250) = convert(varchar, getdate(), 126)
                  declare @param2 table
                (
                  page_num int,
                  ds_medical_product_id int,
                  drugstore_department_id int,
                  country_id int,
                  medical_product_id int,
                  ds_medical_product_str varchar(250),
                  ds_medical_product_tsdate datetime,
                  ds_medical_product_price money,
                  doctor_fio varchar(250),
                  doctor_hours varchar(250),
                  doctor_phone varchar(250),
                  ds_mp_presence_tsdate datetime,
                  presence_id int,
                  order_flag bit,
                  ds_mp_shelf_life datetime
                )

                insert into @param2
                select
                  prm.page_num,
                  ds_mp.ds_medical_product_id,
                  ds_mp.drugstore_department_id,
                  ds_mp.country_id,
                  ds_mp.medical_product_id,
                  ds_mp.ds_medical_product_str,
                  ds_mp.ds_medical_product_tsdate,
                  ds_mp.ds_medical_product_price,
                  ds_mp.doctor_fio,
                  ds_mp.doctor_hours,
                  ds_mp.doctor_phone,
                  ds_mp.ds_mp_presence_tsdate,
                  ds_mp.presence_id,
                  ds_mp.order_flag,
                  ds_mp.ds_mp_shelf_life
                from
                  @t_param prm
                    inner join ds_medical_product ds_mp with(nolock) on prm.medical_product_id      = ds_mp.medical_product_id
                where
                  ds_mp.presence_id = 1 AND ds_mp.ds_mp_presence_tsdate <= getdate()

                declare @block325 varchar(250) = convert(varchar, getdate(), 126);

                declare @t_search table
                (
                  page_num int,
                  search_count int,
                  ds_medical_product_id int,
                  drugstore_department_id int,
                  country_id int,
                  medical_product_id int,
                  ds_medical_product_str varchar(250),
                  ds_medical_product_tsdate datetime,
                  ds_medical_product_price money,
                  doctor_fio varchar(250),
                  doctor_hours varchar(250),
                  doctor_phone varchar(250),
                  ds_mp_presence_tsdate datetime,
                  presence_id int,
                  order_flag bit,
                  ds_mp_shelf_life datetime,
                  medical_product_name_id int,
                  ds_complex_priority float,
                  voice_text varchar(500),
                  half_life int,
                  shelf_life datetime,
                  dd_count int
                )


                insert into @t_search
                select
                  0 as search_count,
                  prm.*,
                  mp.medical_product_name_id,
                  dd_.rnd + 10000000.0*sqrt(
                  (cm.complex_latitude  - cm_dd.complex_latitude )*(cm.complex_latitude  - cm_dd.complex_latitude ) +
                  (cm.complex_longitude - cm_dd.complex_longitude)*(cm.complex_longitude - cm_dd.complex_longitude)
                  ) as ds_complex_priority,
                  dd_v.dd_voice_text as voice_text,
                  case
					  when prm.ds_mp_shelf_life < (GETDATE() + 150.0)
					  then 1
					  else 0
					end as half_life,
				  prm.ds_mp_shelf_life as shelf_life,
				  isnull(dd_cnt.cnt, 0) as dd_count
                from
                  @param2 prm
                    inner join medical_product      mp    with(nolock)              on mp.medical_product_id       = prm.medical_product_id
                    inner join drugstore_department dd    with(nolock)              on dd.drugstore_department_id  = prm.drugstore_department_id
                    inner join complex              cm    with(nolock)              on cm.complex_id               = dd.complex_id
                    inner join area                 ar    with(nolock)              on ar.area_id                  = cm.area_id
                    inner join complex              cm_dd with(nolock)              on cm_dd.complex_id            = @c_id
                    inner join area                 ar_dd with(nolock)              on ar_dd.area_id               = cm_dd.area_id
                    inner join @t_dd                dd_                             on dd_.drugstore_department_id = prm.drugstore_department_id
                    inner join dd_voice				dd_v  with(nolock)				on dd.drugstore_department_id  = dd_v.drugstore_department_id
                    left  join @dd_cnt              dd_cnt                          on dd.drugstore_department_id  = dd_cnt.drugstore_department_id ");
                if(firm_id != null && firm_id > 0)
                    sql += String.Format(" where dd.firm_id = {0} AND dd.dd_outsourcing = 1", firm_id);
                sql += String.Format(@"
                order by
                  presence_id asc,
                  ds_complex_priority asc

                --------------------
                declare @block35 varchar(250) = convert(varchar, getdate(), 126)
                ------------------
                update @t_search set [@t_search].search_count = t.search_count
                from
                (
                select
                  drugstore_department_id,
                  count(*) as search_count
                from
                (
                select
                  drugstore_department_id,
                  medical_product_name_id
                from
                  @t_search
                where
                  presence_id = 1
                group by
                  drugstore_department_id,
                  medical_product_name_id
                ) t
                group by
                  drugstore_department_id
                ) t
                where
                  [@t_search].drugstore_department_id = t.drugstore_department_id

                ------------------------------------------
                declare @block4 varchar(250) = convert(varchar, getdate(), 126)

                declare @t_lh table (load_hdr_id int);
                insert into @t_lh
                select
                  load_hdr_id_calc as load_hdr_id
                from
                  drugstore_department
                where
                  drugstore_department_id in (select distinct drugstore_department_id from @t_search) and not load_hdr_id_calc is null

                declare @t_lsk table (load_str_lek_id int);
                insert into @t_lsk
                select
                  load_str_lek_id
                from
                  load_str_lek
                where
                  medical_product_id in (select distinct medical_product_id from @t_search)
                and
                  load_str_lek_trans_flag = 1
                and
                  load_str_lek_error_flag = 0



                declare @t_search_lsk table
                (
                  drugstore_department_id int,
                  medical_product_id int,
                  load_str_lek_id int,
                  load_str_lek_name varchar(800),
                  load_str_lek_checks int,
                  load_str_lek_trans_flag bit,
                  load_str_lek_error_flag bit
                )

                insert into @t_search_lsk
                select
                  lh.drugstore_department_id,
                  lsk.medical_product_id,
                  lsk.load_str_lek_id,
                  lsk.load_str_lek_name,
                  lsk.load_str_lek_checks,
                  lsk.load_str_lek_trans_flag,
                  lsk.load_str_lek_error_flag
                from
                  load_hs lhs with(nolock)
                    inner join load_str_lek lsk with(nolock) on lsk.load_str_lek_id = lhs.load_str_lek_id
                    inner join load_hdr     lh  with(nolock) on lh.load_hdr_id      = lhs.load_hdr_id
                where
                  lhs.load_hdr_id in (select load_hdr_id from @t_lh)
                and
                  lhs.load_str_lek_id in (select load_str_lek_id from @t_lsk)
                 group by
                  lh.drugstore_department_id,
                  lsk.medical_product_id,
                  lsk.load_str_lek_id,
                  lsk.load_str_lek_name,
                  lsk.load_str_lek_checks,
                  lsk.load_str_lek_trans_flag,
                  lsk.load_str_lek_error_flag

                ------------------------------------------

                declare @block6 varchar(250) = convert(varchar, getdate(), 126)

                --------------------------------------------------

                select top 100 * from (

                select
                  page_num,
                  ds_mp.half_life as hl,
                  convert(varchar,ds_mp.shelf_life,4) as sl,
                  d.drugstore_id as ds_id,
                  d.drugstore_name as ds_name,
                  isnull(dd.dd_voice_file, 0) as dd_voice,
                  dd.drugstore_department_id as dd_id,
                  ltrim(rtrim(cast(dd.drugstore_department_name + ' (' + ltrim(rtrim(cast(dd.drugstore_department_id as varchar))) + ')' as varchar(255)))) as dd_name,
                  dd.drugstore_department_str as dd_str,
                  dd.drugstore_department_tel as dd_tel,
                  cast(dd.drugstore_department_adress+'('+
                  dd.drugstore_department_adress_ref+')' as varchar(250)) as dd_address,
                  mp.medical_product_id as mp_id,
                  mp.medical_product_id as mp_id2,
                  cast(dd.drugstore_department_note as varchar(255)) as dd_note,
                  m_p_n.medical_product_name_id as mpn_id,
                  m_p_n.medical_product_name_name as mpn_name,
                  cast(m_p_n.medical_product_name_note as varchar(255)) as mpn_note,
                  m_f.medical_form_id as mf_id,
                  m_f.medical_form_name as mf_name,
                  cast(replace(replace(replace(mp.medical_product_str + ' ' + mp.medical_product_str2 + ' ' + isnull(mp.medical_product_hours,'') + ' ' + isnull(mp.medical_product_tel,'') + ' ' + isnull(ds_mp.ds_medical_product_str,'') + ' ' + isnull(ds_mp.doctor_fio,'') + ' ' + isnull(ds_mp.doctor_hours,'') + ' ' + isnull(ds_mp.doctor_phone,''), '  ', ' '), '  ', ' '), '  ', ' ') as varchar(255)) as mp_str,
                  ds_mp.ds_medical_product_str as ds_mp_str,
                  i_n.international_name_id as inter_id,
                  i_n.international_name_name as inter_name,
                  pr.presence_name as pr_name,
                  pr.presence_val as pr_val,
                  order_flag,
                  cast(ds_mp.ds_complex_priority as int) as ds_complex_priority,
                  ds_mp.search_count,
                  ds_mp.ds_medical_product_id as ds_mp_id,
                  ds_mp.ds_medical_product_price as ds_mp_price,
                  p.packing_id as p_id,
                  p.packing_name as p_name,
                  c.country_id,
                  c.country_name,
                  convert(varchar,ds_mp.ds_mp_presence_tsdate,4) as ds_mp_pr_tsdate,
                  convert(varchar,ds_mp.ds_medical_product_tsdate,4) as ds_mp_tsdate,
                  ds_mp.ds_mp_presence_tsdate as ds_mp_presence_tsdate,
                  '-----' as stat,
                  ds_mp.load_str_lek_id,
                  isnull(ds_mp.load_str_lek_name,
                    m_p_n.medical_product_name_name + ' ' +
                    m_f.medical_form_name + ' ' +
                    cast(replace(replace(replace(mp.medical_product_str + ' ' + mp.medical_product_str2 + ' ' + isnull(mp.medical_product_hours,'') + ' ' + isnull(mp.medical_product_tel,'') + ' ' + isnull(ds_mp.ds_medical_product_str,'') + ' ' + isnull(ds_mp.doctor_fio,'') + ' ' + isnull(ds_mp.doctor_hours,'') + ' ' + isnull(ds_mp.doctor_phone,''), '  ', ' '), '  ', ' '), '  ', ' ') as varchar(255))
                    ) as load_str_lek_name,
                  ds_mp.load_str_lek_checks,
                  pg.pharmacological_group_id as pg_id,
                  pg.pharmacological_group_name as pg_name,
                  ds_mp.voice_text as voice,
                  ds_mp.dd_count as dd_count,
                  dd.find_key
                from
                (
                select
                  ds_mp.*,
                  lsk.load_str_lek_id,
                  lsk.load_str_lek_name,
                  lsk.load_str_lek_checks
                from
                  @t_search ds_mp
                    left join @t_search_lsk lsk on lsk.medical_product_id      = ds_mp.medical_product_id and
                                                   lsk.drugstore_department_id = ds_mp.drugstore_department_id

                ) ds_mp
                  left join presence              pr    with (nolock) on ds_mp.presence_id             = pr.presence_id
                  left join medical_product       mp    with (nolock) on mp.medical_product_id         = ds_mp.medical_product_id
                  left join medical_product_name  m_p_n with (nolock) on m_p_n.medical_product_name_id = ds_mp.medical_product_name_id
                  left join drugstore_department  dd    with (nolock) on dd.drugstore_department_id    = ds_mp.drugstore_department_id
                  left join complex               cm_dd with (nolock) on cm_dd.complex_id              = dd.complex_id
                  left join area                  ar_dd with (nolock) on ar_dd.area_id                 = cm_dd.area_id
                  left join complex               cm    with (nolock) on cm.complex_id                 = @c_id
                  left join area                  ar    with (nolock) on ar.area_id                    = cm.area_id
                  left join drugstore             d     with (nolock) on dd.drugstore_id               = d.drugstore_id
                  left join medical_form          m_f   with (nolock) on m_f.medical_form_id           = mp.medical_form_id
                  left join international_name    i_n   with (nolock) on i_n.international_name_id     = m_p_n.international_name_id
                  left join pharmacological_group pg    with (nolock) on pg.pharmacological_group_id   = mp.pharmacological_group_calc_id
                  left join packing               p     with (nolock) on p.packing_id                  = mp.packing_id
                  left join country               c     with (nolock) on c.country_id                  = ds_mp.country_id
               ) tab
        ");
            sql += " order by ";
            sql += String.Join(",", this.Sort);

            SqlConnection ch_d_1_dbc = new SqlConnection(Properties.Settings.Default.ch_d_1ConnectionString);
            SqlCommand sc = new SqlCommand(sql, ch_d_1_dbc);
            ch_d_1_dbc.Open();
            SqlDataReader data = sc.ExecuteReader();
            Main.finals.Clear();
            try
            {
                int index = 0;
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        IDataRecord DataRecord = (IDataRecord)data;
                        DrugstoreInfo DD = new DrugstoreInfo(DataRecord);
                        DD.Index = index;
                        index++;
                        Main.finals.Add(DD);
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception except)
            {
                MessageBox.Show("ERROR! " + except.Message.ToString() + except.Source.ToString() + except.TargetSite.ToString());
                return false;
            }
            finally
            {
                ch_d_1_dbc.Dispose();
                data.Close();    
            }
            return true;
        }

        protected string GenerateMpSqlTable()
        {
            if (Main.mps != null && Main.mps.Count > 0)
            {
                string sql = "";
                foreach (KeyValuePair<int, ObservableCollection<MpClass>> kvpair in Main.mps)
                {
                    int index = kvpair.Key;
                    ObservableCollection<MpClass> mps = kvpair.Value;
                    foreach (MpClass mp in mps)
                    {
                        if (mp.Selected == true)
                        {
                            if (sql != "")
                            {
                                sql += String.Format(" union all select {0} page_num, {1} medical_product_name_id, {2} medical_form_id, {3} packing_id, 1 country_id, {4} medical_product_id, {5} medical_product2_id", index, mp.GetMpnId(), mp.GetFormaId(), 1, mp.GetMpId(), mp.GetMpId());
                            }
                            else
                            {
                                sql = String.Format(" select {0} page_num, {1} medical_product_name_id, {2} medical_form_id, {3} packing_id, 1 country_id, {4} medical_product_id, {5} medical_product2_id", index, mp.GetMpnId(), mp.GetFormaId(), 1, mp.GetMpId(), mp.GetMpId());
                            }
                        }
                    }
                }
                return sql;
            }
            else
                return "Ошибка. Нет источника мед. продуктов.";
        }

        private void Forma_SpaceKeyUp(object sender, KeyEventArgs e)
        {
            ListView LV = (ListView)sender;
            switch (e.Key)
            {
                case Key.Space:
                    //Main.DebugText.Text += String.Format("\r\nиндекс - {0}", LV.SelectedIndex);
                    //Main.DebugText.ScrollToEnd();
                    if (LV.SelectedItem is FormaClass)
                    {
                        FormaClass Item = (FormaClass)LV.SelectedItem;
                        int index = Item.Index;
                        if (Item.Selected == false)
                            Main.formas[Main.FormaTabs.SelectedIndex][index].Selected = true;
                        else if (Item.Selected == true)
                            Main.formas[Main.FormaTabs.SelectedIndex][index].Selected = false;
                        //LV.Items.Refresh();
                    }
                    break;
            }
           
        }
        private void MP_SpaceKeyUp(object sender, KeyEventArgs e)
        {
            ListView LV = (ListView)sender;
            switch (e.Key)
            {
                case Key.Space:
                    //Main.DebugText.Text += String.Format("\r\nиндекс - {0}", LV.SelectedIndex);
                    //Main.DebugText.ScrollToEnd();
                    if (LV.SelectedItem is MpClass)
                    {
                        MpClass Item = (MpClass)LV.SelectedItem;
                        int index = Item.Index;
                        if (Item.Selected == false)
                            Main.mps[Main.FormaTabs.SelectedIndex][index].Selected = true;
                        else if (Item.Selected == true)
                            Main.mps[Main.FormaTabs.SelectedIndex][index].Selected = false;
                        //LV.Items.Refresh();
                    }
                    break;
                
            }
        }

        private void MpGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            ListView LV = (ListView)sender;
            LV.SelectedIndex = MpRowIndex;
            MpGrid.SelectedIndex = MpRowIndex;
            //Main.DebugText.Text += String.Format("\r\nmp index - {0}, sel index - {1}", this.MpRowIndex, LV.SelectedIndex);
            //Main.DebugText.ScrollToEnd();
        }

        private void MpGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView LV = (ListView)sender;
            MpRowIndex = LV.SelectedIndex;
        }

    }
}
