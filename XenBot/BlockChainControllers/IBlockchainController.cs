using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XenBot.Entities;

namespace XenBot.BlockChainControllers
{
    public interface IBlockchainController
    {
        public Task<GasPrice> EstimateGasPriceAsync(int tipInPercent);
        public Task<BigInteger> Getbalance(string address);
        public Task<BigInteger> EstimateCoinTransferGas(string from, string to, BigInteger amount);
        public Task<bool> TransferCoins(Nethereum.Web3.Accounts.Account fromAccount, string to, BigInteger amount, int tip, BigInteger gas, GasPrice gasPrice);
        public Task WaitForCoinsToTransfer(string address, BigInteger expected);
        public string ChainName { get; }
        public string Provider { get; init; }
    }
}
