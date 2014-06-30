using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        
        public FinalPage()
        {
            InitializeComponent();
        }
        public FinalPage(SearchMainWindow Wind)
        {
            this.Main = Wind;
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
                        DDIds.Add(final.DDId);
                    }
                }
            }
            AnswerWindow Answer = new AnswerWindow(Texts);
            Answer.Show();
        }

        private void Lists_SpaceKeyUp(object sender, KeyEventArgs e)
        {
            ListView LV = (ListView)sender;
            switch (e.Key)
            {
                case Key.Space:
                    Main.DebugText.Text += String.Format("\r\nиндекс - {0}", LV.SelectedIndex);
                    Main.DebugText.ScrollToEnd();
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
                AnswerWindow AdditionalInfo = new AnswerWindow(Texts, HeaderWindow);
                AdditionalInfo.Show();
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице, чтобы посмотреть дополнительную информацию");
            }
        }
    }
}
