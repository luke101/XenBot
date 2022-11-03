using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DTOs
{
    [FunctionOutput]
    public class UserMintOutputDTO : IFunctionOutputDTO
    {
        [Parameter("address", "address", 1)]
        public virtual string Address { get; set; }

        [Parameter("uint256", "term", 2)]
        public virtual BigInteger Term { get; set; }

        [Parameter("uint256", "maturityTs", 3)]
        public virtual BigInteger MaturityTs { get; set; }

        [Parameter("uint256", "rank", 3)]
        public virtual BigInteger Rank { get; set; }

        [Parameter("uint256", "amplifier", 4)]
        public virtual BigInteger Amplifier { get; set; }

        [Parameter("uint256", "eaaRate", 5)]
        public virtual BigInteger EaaRate { get; set; }
    }
}
