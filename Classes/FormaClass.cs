using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CCsearch
{
    public class FormaClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Index { get; set; }
        public Nullable<bool> Selected { get; set; }
        int Id;
        public string FormaName { get; set; }
        int MpnId;

        public FormaClass(int Id, int MpnId, string Forma, Nullable<bool> IsSelect)
        {
            this.Id = Id;
            this.FormaName = Forma;
            this.Selected = IsSelect;
            this.MpnId = MpnId;
        }

        public int GetId()
        {
            return this.Id;
        }
        public int GetMpnId()
        {
            return this.MpnId;
        }
        public override string ToString()
        {
            return this.FormaName;
        }
        public Nullable<bool> IsSelect()
        {
            return this.Selected;
        }
        public object GetValueByName(string Option)
        {
            switch (Option)
            {
                case "FormaName":
                    return this.FormaName;
                default:
                    return false;
            }
        }
        public void onPropertyChanged(object sender, string propertyName, ObservableCollection<FormaClass> collection )
        {

            if (this.PropertyChanged != null)
            {

                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
                var view = CollectionViewSource.GetDefaultView(collection);
                view.Refresh();
            }

        }
    }
}
