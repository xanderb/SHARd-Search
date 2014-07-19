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
        double ParentWidth = 0.0;
        double ParentHeight = 0.0;
        

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
        public AnswerWindow(ObservableCollection<string> Texts, string Header, double Width, double Height)
        {
            InitializeComponent();
            SimpleAnswerList.ItemsSource = Texts;
            HeaderWindow.Text = Header;
            this.Title = Header;
            ParentWidth = Width;
            ParentHeight = Height;
        }

        private void CloseAnswerWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SimpleAnswerWindow_Loaded(object sender, EventArgs e)
        {
            if (ParentWidth != 0.0 && ParentHeight != 0.0)
            {
                try
                {
                    this.Left = Convert.ToInt32((ParentWidth / 2) - (this.Width / 2));
                    this.Top = Convert.ToInt32((ParentHeight / 2) - (this.Height / 2));
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Не удалось отцентровать окно. Сообщите об ошибке программисту. " + exc.Message);
                    this.Top = 0;
                    this.Left = 0;
                }
            }
        }
    }
}
