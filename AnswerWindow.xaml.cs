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

namespace SHARd.Search
{
    /// <summary>
    /// Логика взаимодействия для AnswerWindow.xaml
    /// </summary>
    public partial class AnswerWindow : Window
    {
        string Header = "Голосовой ответ";
        public AnswerWindow(ObservableCollection<string> Texts)
        {
            InitializeComponent();
            SimpleAnswerList.ItemsSource = Texts;
            this.Title = Header;
        }

        public AnswerWindow(ObservableCollection<string> Texts, string Header)
        {
            InitializeComponent();
            SimpleAnswerList.ItemsSource = Texts;
            HeaderWindow.Text = Header;
            this.Title = Header;
        }

        private void CloseAnswerWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
