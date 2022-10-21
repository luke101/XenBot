using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.WebControllers
{
    public interface IWebController
    {
        public Task<decimal> GetPriceAsync();
    }
}
