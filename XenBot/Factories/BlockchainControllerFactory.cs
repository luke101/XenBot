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
            return new EthBlockchainController("https://rpc.flashbots.net/");
        }

        public IBlockchainController CreateMaticBlockchainController()
        {
            return new MaticBlockchainController("https://polygon-rpc.com/");
        }

        public IBlockchainController CreateFantomBlockchainController()
        {
            return new FantomBlockchainController("https://rpc.ankr.com/fantom/");
        }

        public IBlockchainController CreateEthWBlockchainController()
        {
            return new EthWBlockchainController("https://mainnet.ethereumpow.org");
        }
    }
}
