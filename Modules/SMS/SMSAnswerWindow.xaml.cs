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
using SHARd.Search.DataBaseWork;
using System.Data.SqlClient;

namespace SHARd.Search.Modules.SMS
{
    /// <summary>
    /// Логика взаимодействия для SMSAnswerWindow.xaml
    /// </summary>
    public partial class SMSAnswerWindow : Window
    {
        public SearchMainWindow main { get; set; }
        public Boolean isQuestion = false;
        SMSClass SMSWork;

        public SMSAnswerWindow(SearchMainWindow window, Boolean isQ)
        {
            InitializeComponent();
            this.main = window;
            this.isQuestion = isQ;
            if (this.main.SMSSelected != null)
            {
                SMSWork = main.SMSSelected;
                NumberAnswer.Visibility = Visibility.Hidden;
                AnswerMessage.Visibility = Visibility.Visible;
                MessageText.Text = SMSWork.GetText();
                if (this.isQuestion)
                {
                    WindowHeader.Text = "Отправка уточняющего вопроса";
                    AnswerTextHeader.Text = "Уточняющий вопрос:";
                    SendButton.Content = "Отправить вопрос";
                }
            }
            else
            {
                NumberAnswer.Visibility = Visibility.Visible;
                AnswerMessage.Visibility = Visibility.Hidden;
            }
        }

        public SMSAnswerWindow(SearchMainWindow window, string sendText)
        {
            InitializeComponent();
            this.main = window;
            AnswerTextField.Text = sendText;
            if (this.main.SMSSelected != null)
            {
                SMSWork = main.SMSSelected;
                NumberAnswer.Visibility = Visibility.Hidden;
                AnswerMessage.Visibility = Visibility.Visible;
                MessageText.Text = SMSWork.GetText();
                if (this.isQuestion)
                {
                    WindowHeader.Text = "Отправка уточняющего вопроса";
                    AnswerTextHeader.Text = "Уточняющий вопрос:";
                    SendButton.Content = "Отправить вопрос";
                }
            }
            else
            {
                NumberAnswer.Visibility = Visibility.Visible;
                AnswerMessage.Visibility = Visibility.Hidden;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string sql = "";
            string number = "";
            string text = AnswerTextField.Text.ToString();
            if (text != "" && text != null)
            {
                if (SMSWork == null)
                    number = NumberTo.Text.ToString();
                else
                    number = SMSWork.GetNumber();

                sql = String.Format("EXEC p_sms_add_to_send @message, @number");
                string[] stringSeparators = new string[] {";;"}; //разделитель в тексте для разбивки на отдельные сообщения
                string[] messages = text.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string mg in messages)
                {
                    int messageLength = mg.Length;
                    int messageCount = 1;
                    if(messageLength > 70)
                    {
                        double del = messageLength / 70;
                        messageCount = Convert.ToInt32(Math.Floor(del)) + 1;
                    }
                    for (int i = 1; i <= messageCount; i++ )
                    {
                        
                        int startIndex = (i*70) - 70;
                        int lengthMess = 70;

                        if (i == messageCount) //Если сообщение последнее, отправить сообщение длинной = общее кол-во символов - кол-во итераций * 70
                            lengthMess = messageLength - startIndex;
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        string mess = mg.Substring(startIndex, lengthMess);
                        if (mess != "")
                        {
                            parameters.Add("@message", mess);
                            parameters.Add("@number", number);
                            try
                            {
                                SqlDataReader dr = DataBaseClass.DoProcedureCHD1(sql, parameters);
                            }
                            finally
                            {
                                DataBaseClass.CloseReader();
                                DataBaseClass.CloseConnections();
                            }
                        }
                    }
                }

                if (SMSWork != null)
                {
                    sql = "EXEC p_sms_upd_process @messid, 1";
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("@messid", SMSWork.GetID());
                    try
                    {
                        SqlDataReader dr = DataBaseClass.DoProcedureCHD1(sql, parameters);
                    }
                    finally
                    {
                        DataBaseClass.CloseReader();
                        DataBaseClass.CloseConnections();
                        main.SMSSelected = null;
                    }
                }
                main.SMSInterface.SMSField.Text = "Выбранное СМС сообщение";
                this.Close();
            }
            else
            {
                MessageBox.Show("Напишите текст сообщения!");
            }
        }
    }
}
