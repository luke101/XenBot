using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
namespace XenBot.Entities
{

    //int idOrdinal = reader.GetOrdinal("id");
    //int accountIdOrdinal = reader.GetOrdinal("account_id");
    //int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
    //int tokensOrdinal = reader.GetOrdinal("tokens");
    //int chainOrdinal = reader.GetOrdinal("chain");
    //int nameOrdinal = reader.GetOrdinal("name");
    //int addressOrdinal = reader.GetOrdinal("address");

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
