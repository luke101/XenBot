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
    }
}