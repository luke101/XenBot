using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DatagridEntities
{
    public class ClaimDueVM : INotifyPropertyChanged
    {
        public string Due { get; set; }
        public string Account { get; set; }
        public string Chain { get; set; }
        public string Count { get; set; }
        public string Tokens { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
