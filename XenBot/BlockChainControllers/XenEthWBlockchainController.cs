using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenBot.InputDTOs;
using Nethereum.Contracts;
using System.Security.Principal;
using System.Diagnostics.Contracts;
using XenBot.BlockChainControllers;
using System.Numerics;
using Nethereum.Web3.Accounts;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using XenBot.Entities;

namespace XenBot
{
    public class XenEthWBlockchainController : IXenBlockChainController
    {
        private IBlockchainController _blockchainController;

        private readonly Web3 _web3;
        private readonly string _provider;
        private readonly string _abi;
        private readonly Nethereum.Contracts.Contract _contract;

        public XenEthWBlockchainController(IBlockchainController blockchainController, string abi, string contractAddress)
        {
            _blockchainController = blockchainController;

            _abi = abi;
            _provider = _blockchainController.Provider;
            _web3 = new Web3(_provider);
            _contract = _web3.Eth.GetContract(abi, contractAddress);
        }

        public async Task<UserMintOutputDTO> GetUserMints(string address)
        {
            var userMintFunction = _contract.GetFunction("userMints");
            var userMint = await userMintFunction.CallAsync<UserMintOutputDTO>(address);
            return userMint;
        }

        public long GetGrossReward(long globalRank, long amplifier, long term, long eaa, long rank)
        {
            var delta = Math.Max(globalRank - rank, 2);
            var eea2 = (1_000 + eaa);
            var log128 = Math.Log2(delta);
            var reward128 = log128 * (double)amplifier * (double)term * (double)eea2;
            var r = reward128 / (1_000);
            return (long)r;
        }

        public async Task<long> GetGlobalRank()
        {
            var globalRankFunction = _contract.GetFunction("globalRank");
            long rank = await globalRankFunction.CallAsync<long>();
            return rank;
        }

        public async Task<long> GetCurrentMaxTerm()
        {
            var getCurrentMaxTermFunc = _contract.GetFunction("getCurrentMaxTerm");
            var maxTerm = await getCurrentMaxTermFunc.CallAsync<BigInteger>();
            long maxTermLong = (long)maxTerm;
            long days = maxTermLong / (3600 * 24);
            return days;
        }

        public async Task<BigInteger> EstimateGasToClaim(string address, int days)
        {
            var claimRankFunction = _contract.GetFunction("claimRank");
            var gas = await claimRankFunction.EstimateGasAsync(address, null, null, new BigInteger(days));
            return gas.Value;
        }

        public async Task<BigInteger> EstimateGasToMintReward(string mintAddress, string mainaddress)
        {
            var claimRankFunction = _contract.GetFunction("claimMintRewardAndShare");
            var gas = await claimRankFunction.EstimateGasAsync(mintAddress, null, null, mainaddress, 100);
            return gas.Value;
        }

        public async Task<bool> MintRewardAndShare(Nethereum.Web3.Accounts.Account account, string address, BigInteger gas, GasPrice gasPrice)
        {
            bool success = true;
            Web3 web3 = new Web3(account, _provider);
            web3.TransactionManager.UseLegacyAsDefault = true;
            var contract = web3.Eth.GetContract(_abi, _contract.Address);
            var mintRewardFunction = contract.GetFunction("claimMintRewardAndShare");
            var receipt = await mintRewardFunction.SendTransactionAndWaitForReceiptAsync(account.Address, new HexBigInteger(gas), new HexBigInteger(gasPrice.Priority), value: null, receiptRequestCancellationToken: null, address, 100);

            if (receipt == null)
            {
                throw new Exception("Transaction failed");
            }

            if (receipt.Succeeded(true) == false)
            {
                throw new Exception("Transaction failed");
            }

            return success;
        }

        public async Task WaitForConfirmations(Entities.Transaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            Web3 web3 = new Web3(_provider);

            var currentBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

            while (true)
            {
                await Task.Delay(2000);

                var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                var confirmations = latestBlockNumber.Value - currentBlockNumber;

                if (confirmations >= 2)
                {
                    break;
                }


            }
        }

        public async Task<bool> ClaimRank(Nethereum.Web3.Accounts.Account account, int days, BigInteger gas, GasPrice gasPrice)
        {
            bool success = true;
            Web3 web3 = new Web3(account, _provider);
            web3.TransactionManager.UseLegacyAsDefault = true;
            var contract = web3.Eth.GetContract(_abi, _contract.Address);
            var claimRankFunction = contract.GetFunction("claimRank");
            var receipt = await claimRankFunction.SendTransactionAndWaitForReceiptAsync(account.Address, new HexBigInteger(gas), new HexBigInteger(gasPrice.Priority), value:null, receiptRequestCancellationToken:null, new BigInteger(days));

            if (receipt == null)
            {
                throw new Exception("Transaction failed");
            }

            if (receipt.Succeeded(true) == false)
            {
                throw new Exception("Transaction failed");
            }

            return success;
        }
    }
}
