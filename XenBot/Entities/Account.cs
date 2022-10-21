using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public DateTime? ClaimExpire { get; set; }
        public DateTime? StakeExpire { get; set; }
        public string Address { get; set; }
        public string Chain { get; set; }

        public int? DaysLeft
        {
            get 
            { 
                if(ClaimExpire == null)
                {
                    return null;
                }
                else
                {
                    return (int)(ClaimExpire.Value - DateTime.UtcNow).TotalDays;
                }
            }
        }
    }
}
