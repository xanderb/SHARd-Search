using System;
using System.Collections.Generic;
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

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DrugstoreInfo final in Main.finals)
            {
                if (final.Selected == true)
                {
                    Dispatcher.Invoke(
                        () =>
                        {
                            Main.DebugText.Text += String.Format("\r\nВыбрана аптека - {0}, индекс строки - {1}", final.DDName, final.Index);
                        }
                    );
                }
            }
        }
    }
}
