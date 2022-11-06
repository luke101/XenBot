using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DatagridEntities
{
    public class ClaimVM : INotifyPropertyChanged
    {
        private DateTime? _claimExpire;

        public int AccountId { get; set; }

        public DateTime? ClaimExpire 
        {
            set
            {
                if (!value.Equals(_claimExpire))
                {
                    _claimExpire = value;
                    NotifyPropertyChanged(nameof(ClaimExpire));
                }
            }
            get
            {
                return _claimExpire;
            }
        }

        private DateTime? _stakeExpire;
        public DateTime? StakeExpire
        {
            set
            {
                if (!value.Equals(_stakeExpire))
                {
                    _stakeExpire = value;
                    NotifyPropertyChanged(nameof(StakeExpire));
                }
            }
            get
            {
                return _stakeExpire;
            }
        }

        private int? _daysLeft;
        public int? DaysLeft
        {
            set
            {
                if (!value.Equals(_daysLeft))
                {
                    _daysLeft = value;
                    NotifyPropertyChanged(nameof(DaysLeft));
                }
            }
            get
            {
                return _daysLeft;
            }
        }

        public string Address { get; set; }
        public string Chain { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
