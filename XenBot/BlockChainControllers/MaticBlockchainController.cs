using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Bcpg;
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
    public class MaticBlockchainController : IBlockchainController
    {
        private readonly string _provider;
        private readonly Web3 _web3;

        public string ChainName { get => "MATIC"; }
        public int ChainId { get => 137; }
        public string Provider { get; init; }

        public MaticBlockchainController(string provider)
        {
            _provider = provider;
            _web3 = new Web3(provider);
            Provider = provider;
        }

        public async Task<GasPrice> EstimateGasPriceAsync(int tipInPercent)
        {
            HexBigInteger gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
            decimal gasPriceM = (decimal)gasPrice.Value;
            decimal percentage = (decimal)tipInPercent / 100;
            decimal priority = gasPriceM + (gasPriceM * percentage);
            GasPrice gp = new GasPrice() { Price = gasPrice.Value, Priority = new BigInteger(priority) };
            return gp;
        }

        public async Task<BigInteger> Getbalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return balance.Value;
        }

        public async Task WaitForCoinsToTransfer(string address, BigInteger expected)
        {
            DateTime timeout = DateTime.Now.AddMinutes(5);

            while (true)
            {
                var bal = await Getbalance(address);

                if (bal >= expected)
                {
                    break;
                }

                if (DateTime.Now >= timeout)
                {
                    throw new Exception("waiting too long for transfer of coins");
                }

                await Task.Delay(1000);
            }
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

        public async Task<Entities.Transaction?> TransferCoins(Nethereum.Web3.Accounts.Account fromAccount, string to, BigInteger amount, int tip, BigInteger gas, GasPrice gasPrice)
        {
            Entities.Transaction? transaction = null;

            if (amount <= new BigInteger(0))
            {
                return transaction;
            }



            bool success = true;

            Nethereum.Web3.Accounts.Account account = new Nethereum.Web3.Accounts.Account(fromAccount.PrivateKey, 137);

            Web3 web3 = new Web3(account, _provider);

            var ether = Web3.Convert.FromWei(amount);
            var gasPriceGwei = Web3.Convert.FromWei(gasPrice.Price, Nethereum.Util.UnitConversion.EthUnit.Gwei);

            //var transferFunction = new TransferFunction();
            //transferFunction.FromAddress = fromAccount.Address;
            //transferFunction.To = "0xd8da6bf26964af9d7eed9e03e53415d37aa96045"
            //transferFunction.Value = 500;

            transaction = new Entities.Transaction();

            if (amount > new BigInteger(0))
            {
                //var transactionInput = new TransactionInput
                //{
                //    From = fromAccount.Address,
                //    To = to,
                //    Value = new HexBigInteger(amount),
                //    Gas = new HexBigInteger(gas),
                //    GasPrice = new HexBigInteger(gasPrice.Price),
                //    MaxFeePerGas = new HexBigInteger(gasPrice.Priority)
                //};

                var transactionReceipt = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(to, ether, gasPriceGwei, gas, null, null);

                success = transactionReceipt.Succeeded(true);

                if (success == false)
                {
                    throw new Exception("Failed transferring coins");
                }

                transaction.BlockNumber = transactionReceipt.BlockNumber.Value;
                transaction.TransactionHash = transaction.TransactionHash;
            }

            return transaction;
        }

        public async Task<decimal> LoadBalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value);
        }
    }
}
