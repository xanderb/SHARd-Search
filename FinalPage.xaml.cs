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
            if (e.Key == Key.Space)
            {
                ListView LV = (ListView)sender;

                if (LV.SelectedItem is DrugstoreInfo)
                {
                    DrugstoreInfo Item = (DrugstoreInfo)LV.SelectedItem;
                    int index = Item.Index;
                    if (Item.Selected == false)
                        Main.finals[index].Selected = true;
                    else if (Item.Selected == true)
                        Main.finals[index].Selected = false;
                    LV.Items.Refresh();
                }
                
            }
        }
    }
}
