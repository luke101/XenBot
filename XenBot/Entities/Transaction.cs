using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.Entities
{
    public class Transaction
    {
        public System.Numerics.BigInteger BlockNumber { get; set; }
        public string TransactionHash { get; set; }
    }
}
