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

namespace SHARd.Search.Modules.SMS
{
    /// <summary>
    /// Логика взаимодействия для SMS.xaml
    /// </summary>
    public partial class SMS : UserControl
    {
        public SMS()
        {
            InitializeComponent();
        }

        private void SMSField_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Mouse Click Release!");
        }
    }
}
