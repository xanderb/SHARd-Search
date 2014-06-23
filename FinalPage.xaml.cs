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

namespace CCsearch
{
    /// <summary>
    /// Логика взаимодействия для FinalPage.xaml
    /// </summary>
    public partial class FinalPage : UserControl
    {
        public MainWindow Main { get; set; }
        public delegate void AutoAnswerDelegate(List<int> DrugstoreIds); //делегат для события автоответа со списком ID выбранных аптек

        public event AutoAnswerDelegate onAutoAnswer; //событие автоответа

        public FinalPage()
        {
            InitializeComponent();
        }
        public FinalPage(MainWindow Wind)
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
            //Сбор данных и инициализация события автоответа
            List<int> DrugstoreIds = new List<int>();
            foreach (DrugstoreInfo final in Main.finals)
            {
                if (final.Selected == true)
                {
                    if (DrugstoreIds.IndexOf(final.DDId) < 0)
                    {
                        DrugstoreIds.Add(final.DDId);
                    }
                }
            }
            onAutoAnswer(DrugstoreIds);
        }
    }
}
