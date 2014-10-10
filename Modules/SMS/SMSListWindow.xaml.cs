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
using System.Windows.Shapes;

namespace SHARd.Search.Modules.SMS
{
    /// <summary>
    /// Логика взаимодействия для SMSListWindow.xaml
    /// </summary>
    public partial class SMSListWindow : Window
    {
        SearchMainWindow main;
        SMS smsForm;

        public SMSListWindow(SearchMainWindow obj, SMS sms)
        {
            this.main = obj;
            this.smsForm = sms;
            InitializeComponent();
            SMSList.ItemsSource = main.sms_messages;

            if(main.SMSSelected != null)
                SMSList.SelectedIndex = main.SMSSelected.Index;
            
        }

        private void SMSList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void SMSListItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            SMSClass smsMess = (SMSClass)SMSList.SelectedItem;
            if (smsMess != null)
            {
                this.SMSResetSelection();
                smsMess.selected = true;
                main.SMSSelected = smsMess;

                Dispatcher.InvokeAsync(() =>
                {
                    smsForm.SMSField.Text = main.SMSSelected.GetText();
                });
                this.Visibility = Visibility.Hidden;
            }
        }

        private void SMSCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void SMSResetButton_Click(object sender, RoutedEventArgs e)
        {
            this.SMSResetSelection();
            Dispatcher.InvokeAsync(() =>
            {
                smsForm.SMSField.Text = "Выбранное СМС сообщение";
            });
            this.Close();
        }

        /*
         * Сброс выбора СМС сообщения
         */
        public void SMSResetSelection()
        {
            foreach (SMSClass sms in main.sms_messages)
            {
                sms.selected = false;
                main.SMSSelected = null;
            }
        }

    }
}
