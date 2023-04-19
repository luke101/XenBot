using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.Utils
{
    public class DateUtils
    {
        public static DateTime ConvertUnixTimestampToDate(long unixTimeStamp)
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
            return offset.UtcDateTime;
        }
    }
}
