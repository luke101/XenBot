using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DataControllers
{
    public class AccountEventArg : EventArgs
    {
        public Entities.Account Account { get; init; }

        public AccountEventArg(Entities.Account account)
        {
            Account = account;
        }
    }
}
