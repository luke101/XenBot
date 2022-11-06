using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DatagridEntities
{
    public class AccountVM : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public bool Selected { get; set; } = false;

        private string _name;

        public string Name 
        {
            set
            {
                if (!value.Equals(_name))
                {
                    _name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
            get
            {
                return _name;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
