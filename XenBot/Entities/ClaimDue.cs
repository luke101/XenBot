using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.Entities
{
    public class ClaimDue
    {
        public string DueName { get; set; }
        public string Account { get; set; }
        public string Chain { get; set; }
        public int Count { get; set; }
        public long Tokens { get; set; }
    }
}
