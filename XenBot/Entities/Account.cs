﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime ClaimExpire { get; set; }
        public string Name { get; set; }
        public string Phrase { get; set; }
        public string Address { get; set; }
    }
}
