using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DTOs
{
    [Function("claimRank")]
    public class ClaimRankFunction : FunctionMessage
    {
        [Parameter("uint256", "term", 1)]
        public BigInteger Term { get; set; }
    }
}
