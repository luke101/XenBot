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
                    NotifyPropertyChanged(nameof(ExpireTime));
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

        private int? _secondsLeft;
        public int? SecondsLeft
        {
            get
            {
                int seconds = 0;

                if (ClaimExpire.HasValue)
                {
                    var ts = new TimeSpan(_claimExpire.Value.Ticks - DateTime.Now.Ticks);
                    seconds = (int)ts.TotalSeconds;
                }

                return seconds;
            }
        }

        private string _expireTime;
        public string ExpireTime
        {
            get
            {
                string expire = "NA";

                if(_claimExpire.HasValue && _claimExpire > DateTime.MinValue)
                {
                    const int SECOND = 1;
                    const int MINUTE = 60 * SECOND;
                    const int HOUR = 60 * MINUTE;
                    const int DAY = 24 * HOUR;
                    const int MONTH = 30 * DAY;

                    var ts = new TimeSpan(_claimExpire.Value.Ticks - DateTime.Now.Ticks);
                    double delta = Math.Abs(ts.TotalSeconds);

                    if(ts.TotalSeconds > 0)
                    {
                        if (delta < 1 * MINUTE)
                        {
                            expire = ts.Seconds == 1 ? "in one second" : "In " + ts.Seconds + " seconds";
                        }
                        else if (delta < 2 * MINUTE)
                        {
                            expire = "In 1 minute";
                        }
                        else if (delta < 45 * MINUTE)
                        {
                            expire = "In " + ts.Minutes + " minutes";
                        }
                        else if (delta < 90 * MINUTE)
                        {
                            expire = "in an hour";
                        }
                        else if (delta < 24 * HOUR)
                        {
                            expire = "In " + ts.Hours + " hours";
                        }
                        else if (delta < 48 * HOUR)
                        {
                            expire = "Tomorrow";
                        }
                        else if (delta < 30 * DAY)
                        {
                            expire = "In " + ts.Days + " days";
                        }
                        else if (delta < 12 * MONTH)
                        {
                            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                            expire = months <= 1 ? "In one month" : "In " + months + " months";
                        }
                        else
                        {
                            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                            expire = years <= 1 ? "In one year" : "In " + years + " years";
                        }
                    }
                    else
                    {
                        if (delta < 1 * MINUTE)
                        {
                            expire = ts.Seconds == 1 ? "Due one second ago" : "Due " + Math.Abs(ts.Seconds) + " seconds ago";
                        }
                        else if (delta < 2 * MINUTE)
                        {
                            expire = "Due 1 minute ago";
                        }
                        else if (delta < 45 * MINUTE)
                        {
                            expire = "Due " + Math.Abs(ts.Minutes) + " minutes ago";
                        }
                        else if (delta < 90 * MINUTE)
                        {
                            expire = "Due an hour ago";
                        }
                        else if (delta < 24 * HOUR)
                        {
                            expire = "Due " + Math.Abs(ts.Hours) + " hours ago";
                        }
                        else if (delta < 48 * HOUR)
                        {
                            expire = "Due yesterday";
                        }
                        else if (delta < 30 * DAY)
                        {
                            expire = "Due " + Math.Abs(ts.Days) + " days ago";
                        }
                        else if (delta < 12 * MONTH)
                        {
                            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                            expire = months <= 1 ? "Due one month ago" : "Due " + Math.Abs(months) + " months ago";
                        }
                        else
                        {
                            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                            expire = years <= 1 ? "Due one year ago" : "Due " + Math.Abs(years) + " years ago";
                        }
                    }
                }

                return expire;
            }
        }

        private string _estimatedTokens;
        public string EstimatedTokens
        {
            set
            {
                if (!value.Equals(_estimatedTokens))
                {
                    _estimatedTokens = value;
                    NotifyPropertyChanged(nameof(EstimatedTokens));
                }
            }
            get
            {
                return _estimatedTokens;
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
