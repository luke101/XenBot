using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenBot.BlockChainControllers;

namespace XenBot.Factories
{
    public class BlockchainControllerFactory
    {
        public IBlockchainController CreateBscBlockchainController()
        {
            return new BscBlockchainController("https://bsc-dataseed.binance.org/");
        }

        public IBlockchainController CreateEthBlockchainController()
        {
            //https://rpc.flashbots.net/
            //return new EthBlockchainController("https://mainnet.infura.io/v3/8a63c898226d41558163dadbf3c6b7c0");
            return new EthBlockchainController("https://nodes.mewapi.io/rpc/eth");
        }

        public IBlockchainController CreateMaticBlockchainController()
        {
            return new MaticBlockchainController("https://polygon-rpc.com/");
            //return new MaticBlockchainController("https://matic-mainnet-full-rpc.bwarelabs.com");
        }

        public IBlockchainController CreateFantomBlockchainController()
        {
            return new FantomBlockchainController("https://rpc.ankr.com/fantom/");
        }

        public IBlockchainController CreateEthWBlockchainController()
        {
            return new EthWBlockchainController("https://mainnet.ethereumpow.org");
        }

        public IBlockchainController CreateDogechainBlockchainController()
        {
            return new DogechainBlockchainController("https://rpc-sg.dogechain.dog");
        }

        public IBlockchainController CreateAvalancheBlockchainController()
        {
            return new AvalancheBlockchainController("https://api.avax.network/ext/bc/C/rpc");
        }

        public IBlockchainController CreateMoonbeamBlockchainController()
        {
            return new MoonbeamBlockchainController("https://rpc.ankr.com/moonbeam");
        }

        public IBlockchainController CreateEvmosBlockchainController()
        {
            return new EvmosBlockchainController("https://eth.bd.evmos.org:8545");
        }

        public IBlockchainController CreateOKXChainBlockchainController()
        {
            return new OKXChainBlockchainController("https://exchainrpc.okex.org");
        }
    }
}
