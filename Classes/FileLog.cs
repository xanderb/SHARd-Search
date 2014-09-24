using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;

namespace SHARd.Search
{
    public class FileLog
    {
        private string FilePath;
        private string ComputerName;
        private DirectoryInfo dirD;
        private string FileName;
        private int userId = -1;
        private StreamWriter sw;

        public FileLog(string path)
        {
            ComputerName = Environment.MachineName;
            FileName = DateTime.Now.ToString().Replace(":", ".")+".log";
            if (path != "")
            {
                dirD = new DirectoryInfo(@path + "\\" + ComputerName);
                if (dirD.Exists)
                    this.FilePath = @path+"\\"+ComputerName;
                else
                {
                    dirD.Create();
                    this.FilePath = @path+"\\"+ComputerName;
                }
            }
            else
            {
                this.FilePath = Properties.Settings.Default.log_directory.ToString() + ComputerName;
                dirD = new DirectoryInfo(@FilePath);
                if (!dirD.Exists)
                    dirD.Create();
            }
            sw = new StreamWriter(File.OpenWrite(FilePath + "\\" + FileName));
            sw.AutoFlush = true;
        }

        public void AddUser(int user)
        {
            userId = user;
        }

        public void Log(string logRow)
        {
            if (dirD.Exists)
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    if (userId != -1)
                    {
                        this.sw.WriteLine(String.Format("{0} - {1} || {2}", DateTime.Now.ToLocalTime(), userId, logRow));
                    }
                    else
                    {
                        this.sw.WriteLine(String.Format("{0} || {1}", DateTime.Now.ToLocalTime(), logRow));
                    }
                });
            }
        }

        public void CloseLogStream()
        {
            sw.WriteLine(String.Format("{0} || Запись лога закончена", DateTime.Now.ToLocalTime()));
            this.sw.Close();
        }
    }
}
