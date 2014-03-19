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
            CheckBox Cb = sender as CheckBox;
            int CbInd = Convert.ToInt32(Cb.Tag.ToString());
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (Main.FormaTabs != null && Main.FormaTabs.SelectedIndex != -1)
                {
                    int TabInd = Main.FormaTabs.SelectedIndex;
                    MessageBox.Show(String.Format("Индекс таба - {0}, индекс чекбокса - {1}, значение - {2}", TabInd, CbInd, Main.formas[TabInd][CbInd].ToString()));
                }
            }));
        }
    }
}
