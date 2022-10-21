using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XenBot.InputDTOs;

namespace XenBot.BlockChainControllers
{
    public interface IXenBlockChainController
    {
        public Task<UserMintOutputDTO> GetUserMints(string address);
        public Task<long> GetCurrentMaxTerm();
        public Task<BigInteger> EstimateGasToClaim(string address, int days);
        public Task<bool> ClaimRank(Account account, int days, int tip);
    }
}
