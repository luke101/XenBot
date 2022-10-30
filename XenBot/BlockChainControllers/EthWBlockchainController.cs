using NBitcoin.RPC;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XenBot.Entities;

namespace XenBot.BlockChainControllers
{
    public class EthWBlockchainController : IBlockchainController
    {
        private readonly string _provider;
        private readonly Web3 _web3;

        public string ChainName { get => "EthereumPOW"; }
        public string Provider { get; init; }

        public EthWBlockchainController(string provider)
        {
            _provider = provider;
            _web3 = new Web3(provider);
            Provider = provider;
        }

        public async Task<GasPrice> EstimateGasPriceAsync(int tipInPercent)
        {
            HexBigInteger gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
            decimal gasPriceM = (decimal)gasPrice.Value;
            decimal percentage = tipInPercent / 100;
            decimal priority = gasPriceM + (gasPriceM * percentage);
            GasPrice gp = new GasPrice() { Price = gasPrice.Value, Priority = new BigInteger(priority) };
            return gp;
        }

        public async Task WaitForCoinsToTransfer(string address, BigInteger expected)
        {
            DateTime timeout = DateTime.Now.AddMinutes(5);

            while (true)
            {
                var bal = await Getbalance(address);

                if(bal >= expected)
                {
                    break;
                }

                if(DateTime.Now >= timeout)
                {
                    throw new Exception("waiting too long for transfer of coins");
                }

                await Task.Delay(1000);
            }
        }

        public async Task<BigInteger> Getbalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return balance.Value;
        }

        public async Task<BigInteger> EstimateCoinTransferGas(string from, string to, BigInteger amount)
        {
            CallInput input = new CallInput();
            input.From = from;
            input.To = to;
            input.Value = new HexBigInteger(amount);

            var estimate = await _web3.Eth.Transactions.EstimateGas.SendRequestAsync(input);

            return estimate.Value;
        }

        public async Task<bool> TransferCoins(Nethereum.Web3.Accounts.Account fromAccount, string to, BigInteger amount, int tip, BigInteger gas, GasPrice gasPrice)
        {
            if (amount <= new BigInteger(0))
            {
                return true;
            }

            bool success = true;

            Web3 web3 = new Web3(fromAccount, _provider);

            web3.TransactionManager.UseLegacyAsDefault = true;

            if (amount > new BigInteger(0))
            {
                var transactionInput = new TransactionInput
                {
                    From = fromAccount.Address,
                    To = to,
                    Value = new HexBigInteger(amount),
                    Gas = new HexBigInteger(gas),
                    GasPrice = new HexBigInteger(gasPrice.Price)
                };

                var transactionReceipt = await web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(transactionInput, null);

                success = transactionReceipt.Succeeded(true);
            }

            return success;
        }

        public async Task<decimal> LoadBalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value);
        }
    }
}
