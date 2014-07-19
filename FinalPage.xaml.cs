using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SHARd.Search
{
    /// <summary>
    /// Логика взаимодействия для FinalPage.xaml
    /// </summary>
    public partial class FinalPage : UserControl
    {
        public SearchMainWindow Main { get; set; }
        public SecondPage Second { get; set; }
        
        public FinalPage()
        {
            InitializeComponent();
        }
        public FinalPage(SearchMainWindow Wind, SecondPage Second)
        {
            this.Main = Wind;
            this.Second = Second;
            InitializeComponent();
        }
        private void FinalGrid_MouseDoubleClick(object sender, MouseButtonEventArgs args) {  }

        private void SimpleAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> Texts = new ObservableCollection<string>();
            List<int> DDIds = new List<int>();
            foreach (DrugstoreInfo final in Main.finals)
            {
                if (final.Selected == true)
                {
                    if (DDIds.IndexOf(final.DDId) < 0)
                    {
                        Texts.Add(String.Format("{0} - {1} - {2}", final.DDName, final.DDAddress, final.DDTel));
                        Main.FLog.Log(String.Format("FinalPage - SimpleAnswerButton_Click. Выбрана аптека на ответ голосом, {0} - {1} - {2}", final.DDName, final.DDAddress, final.DDTel));
                        DDIds.Add(final.DDId);
                    }
                }
            }
            AnswerWindow Answer = new AnswerWindow(Texts, "Ответ голосом", Main.Width, Main.Height);
            Answer.Show();
        }

        private void Lists_SpaceKeyUp(object sender, KeyEventArgs e)
        {
            ListView LV = (ListView)sender;
            switch (e.Key)
            {
                case Key.Space:
                    //Main.DebugText.Text += String.Format("\r\nиндекс - {0}", LV.SelectedIndex);
                    //Main.DebugText.ScrollToEnd();
                    if (LV.SelectedItem is DrugstoreInfo)
                    {
                        DrugstoreInfo Item = (DrugstoreInfo)LV.SelectedItem;
                        int index = Item.Index;
                        if (Item.Selected == false)
                            Main.finals[index].Selected = true;
                        else if (Item.Selected == true)
                            Main.finals[index].Selected = false;
                    }
                    break;
                
            }
        }

        private void AutoAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            Main.AutoAnswerButton_Click(sender, e);
        }

        private void AddInfoButton_Click(object sender, RoutedEventArgs e)
        {
            int SelectedRow = FinalGrid.SelectedIndex;
            if (SelectedRow > -1)
            {
                DrugstoreInfo DI = Main.finals[SelectedRow];
                string HeaderWindow = "Дополнительная информация";
                ObservableCollection<string> Texts = new ObservableCollection<string>();
                Texts.Add(String.Format("Наименование учреждения - {0}, {1}, {2}", DI.DDName, DI.DDTel, DI.DDAddress));
                Texts.Add(String.Format("Описание учреждения - {0}", DI.DDNote));
                Texts.Add(String.Format("Наименование мед. препарата - {0}", DI.MpnName));
                Texts.Add(String.Format("Международное наменование - {0}", DI.InterName));
                Texts.Add(String.Format("Фармакологическая группа - {0}", DI.PgName));
                Texts.Add(String.Format("Описание препарата - {0}", DI.MpnNote));
                AnswerWindow AdditionalInfo = new AnswerWindow(Texts, HeaderWindow, Main.Width, Main.Height);
                AdditionalInfo.Show();
                Main.FLog.Log(String.Format("Показана дополнительная информация об аптеке {0}({1})", DI.DDName, DI.DDId));
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице, чтобы посмотреть дополнительную информацию");
            }
        }

        public string FilterTextParse()
        {
            string text = "";
            if (FilterText.Text != "")
            {
                string FilterTextSource = FilterText.Text;
                FilterTextSource = FilterTextSource.Trim().ToLower();
                FilterTextSource = Regex.Replace(FilterTextSource, "\\s+", " ", RegexOptions.IgnoreCase);
                string FieldName = "load_str_lek_name";
                text = " ( ";
                string[] Words = FilterTextSource.Split(' ');

                foreach(string word in Words)
                {
                    switch(word)
                    {
                        case "или":
                            text += " OR ";
                            break;
                        case "и":
                            text += " ) AND ( ";
                            break;
                        case " ":
                            break;
                        default:
                            text += " " + FieldName + " LIKE '%" + word + "%' ";
                            break;
                    }
                }

                text += " ) ";
                Main.FLog.Log(String.Format("Парсинг строки фильтра на финальной странице. Текст: {0}", FilterTextSource));

            }
            return text;
        }

        private void FilterSubmit_Click(object sender, RoutedEventArgs e)
        {
            Main.finals.Clear();
            string genericSql = Second.GenerateMpSqlTable();
            string FilterSql = FilterTextParse();
            if(FilterSql != "")
            {
                if (Second.GetFinalInfo(genericSql, 0, FilterSql))
                {
                    Main.FLog.Log(String.Format("Применен фильтр на финальной странице. Поиск по выбранному городу"));
                }
                else if (Second.GetFinalInfo(genericSql, 1, FilterSql))
                {
                    Main.FLog.Log(String.Format("Применен фильтр на финальной странице. Поиск по всем городам"));
                }
            }
        }

        private void SortDist_Click(object sender, RoutedEventArgs e)
        {
            Second.Sort = new string[]
            {           
                "find_key desc",
                "ds_complex_priority asc",
                "search_Count desc",
                "dd_count asc",
                "ds_name",
                "dd_name",
                "mpn_name",
                "mf_name",
                "p_name",
                "mp_str"
            };
            Main.finals.Clear();
            string genericSql = Second.GenerateMpSqlTable();
            if (Second.GetFinalInfo(genericSql, 0, null))
            {
                Main.FLog.Log(String.Format("Сортировка по дистанции на финальной странице. Поиск по выбранному городу"));
            }
            else if (Second.GetFinalInfo(genericSql, 1, null))
            {
                Main.FLog.Log(String.Format("Сортировка по дистанции на финальной странице. Поиск по всем городам"));
            }
        }

        private void SortNalDist_Click(object sender, RoutedEventArgs e)
        {
            Second.Sort = new string[]
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
            Main.finals.Clear();
            string genericSql = Second.GenerateMpSqlTable();
            if (Second.GetFinalInfo(genericSql, 0, null))
            {
                Main.FLog.Log(String.Format("Сортировка по наличию и дистанции на финальной странице. Поиск по выбранному городу"));

            }
            else if (Second.GetFinalInfo(genericSql, 1, null))
            {
                Main.FLog.Log(String.Format("Сортировка по наличию и дистанции на финальной странице. Поиск по всем городам"));

            }
        }

        private void SortDate_Click(object sender, RoutedEventArgs e)
        {
            Second.Sort = new string[]
            {
                "ds_mp_presence_tsdate desc",
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
            Main.finals.Clear();
            string genericSql = Second.GenerateMpSqlTable();
            if (Second.GetFinalInfo(genericSql, 0, null))
            {
                Main.FLog.Log(String.Format("Сортировка по дате добавления на финальной странице. Поиск по выбранному городу"));

            }
            else if (Second.GetFinalInfo(genericSql, 1, null))
            {
                Main.FLog.Log(String.Format("Сортировка по дате добавления на финальной странице. Поиск по всем городам"));

            }
        }
    }
}
