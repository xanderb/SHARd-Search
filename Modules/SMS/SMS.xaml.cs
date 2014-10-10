using SHARd.Search.DataBaseWork;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
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
        public SearchMainWindow main { get; set; }
        public SMSListWindow listing;

        public SMS()
        {
            InitializeComponent();
            //this.listing.Closing += ListingClosed;
        }

        private void ListingClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.listing = null;
            //this.listing.Closing += ListingClosed;
        }

        private void SMSField_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.listing = new SMSListWindow(main, this);
            this.listing.ShowDialog();
        }

        private void SMSListOpen_Click(object sender, RoutedEventArgs e)
        {
            this.listing = new SMSListWindow(main, this);
            this.listing.ShowDialog();
        }

        public void SMSCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.listing != null)
            {
                if (main.SMSSelected != null)
                {
                    this.listing.SMSList.SelectedIndex = main.SMSSelected.Index;
                }
            }
        }

        private void SMSQuestion_Click(object sender, RoutedEventArgs e)
        {
            if(main.SMSSelected != null)
            {
                SMSAnswerWindow answerWindow = new SMSAnswerWindow(main, true);
                answerWindow.Show();
            }
            else
            {
                MessageBox.Show(String.Format("Необходимо выбрать смс для вопроса"));
            }
        }

        private void SMSIgnor_Click(object sender, RoutedEventArgs e)
        {
            if (main.SMSSelected != null)
            {
                int id = main.SMSSelected.GetID();
                int status = 1;
                string sql = "EXEC p_sms_upd_to_spam @id, @status";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@id", id);
                parameters.Add("@status", status);

                try
                {
                    SqlDataReader dr = DataBaseClass.DoProcedureCHD1(sql, parameters);
                }
                finally
                {
                    DataBaseClass.CloseReader();
                    DataBaseClass.CloseConnections();
                    SMSField.Text = "Выбранное СМС сообщение";
                    main.SMSSelected = null;
                }
                
            }
            else
            {
                MessageBox.Show("Выберите СМС");
            }
        }

        private void SMSSpam_Click(object sender, RoutedEventArgs e)
        {
            if (main.SMSSelected != null)
            {
                int id = main.SMSSelected.GetID();
                string tel = main.SMSSelected.GetNumber();
                string sql = "EXEC p_sms_add_num_to_spam @id, @tel";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@id", id);
                parameters.Add("@tel", tel);

                try
                {
                    SqlDataReader dr = DataBaseClass.DoProcedureCHD1(sql, parameters);
                }
                finally
                {
                    DataBaseClass.CloseReader();
                    DataBaseClass.CloseConnections();
                    SMSField.Text = "Выбранное СМС сообщение";
                    main.SMSSelected = null;
                }
                
            }
            else
            {
                MessageBox.Show("Выберите СМС");
            }
        }
    }
}
