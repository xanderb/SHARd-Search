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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CCsearch
{
    /// <summary>
    /// Логика взаимодействия для AnswerWindow.xaml
    /// </summary>
    public partial class AnswerWindow : Window
    {
        public AnswerWindow(ObservableCollection<string> Texts)
        {
            InitializeComponent();
            SimpleAnswerList.ItemsSource = Texts;
        }

        private void CloseAnswerWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
