﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenBot.WebControllers;

namespace XenBot.Factories
{
    public class WebControllerFactory
    {
        public IWebController CreateBscWebController()
        {
            return new BscWebController();
        }

        public IWebController CreateEthWebController()
        {
            return new EthWebController();
        }

        public IWebController CreateMaticWebController()
        {
            return new MaticWebController();
        }

        public IWebController CreateFantomWebController()
        {
            return new FantomWebController();
        }

        public IWebController CreateEthWWebController()
        {
            return new EthWWebController();
        }

        public IWebController CreateDogechainWebController()
        {
            return new DogechainWebController();
        }

        public IWebController CreateAvalancheWebController()
        {
            return new AvalancheWebController();
        }

        public IWebController CreateMoonbeamWebController()
        {
            return new MoonbeamWebController();
        }

        public IWebController CreateEvmosWebController()
        {
            return new EvmosWebController();
        }

        public IWebController CreateOKXChainWebController()
        {
            return new OKXChainWebController();
        }
    }
}
