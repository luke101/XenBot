using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenBot.DataControllers
{
    public class DataController
    {
        private string _dbFileName;

        public DataController()
        {
            _dbFileName = "data.db";
        }

        public long GetTotalClaims()
        {
            long count = 0;

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select count(*) from data";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    count = ((long?)command.ExecuteScalar()).Value;
                }
            }

            return count;
        }

        public List<Entities.Account> GetAccounts()
        {
            List<Entities.Account> accounts = new List<Entities.Account>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain from data order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
                        int stakeExpireOrdinal = reader.GetOrdinal("stake_expire");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int chainOrdinal = reader.GetOrdinal("chain");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.AccountId = reader.GetInt32(idOrdinal);
                            account.ClaimExpire = reader.IsDBNull(claimExpireOrdinal) ? null : reader.GetDateTime(claimExpireOrdinal).ToLocalTime();
                            account.StakeExpire = reader.IsDBNull(stakeExpireOrdinal) ? null : reader.GetDateTime(stakeExpireOrdinal).ToLocalTime();
                            account.Address = reader.GetString(addressOrdinal);
                            account.Chain = reader.GetString(chainOrdinal);
                            accounts.Add(account);
                        }

                    }
                }
            }

            return accounts;
        }

        public Dictionary<string, int> AggregateAccountsByChain()
        {
            Dictionary<string, int> chainDict = new Dictionary<string, int>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select chain, count(*) as count from data group by chain";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int countOrdinal = reader.GetOrdinal("count");

                        while (reader.Read())
                        {
                            chainDict[reader.GetString(chainOrdinal)] = reader.GetInt32(countOrdinal);
                        }
                    }
                }
            }

            return chainDict;
        }

        public void CreateDFile()
        {
            if (!File.Exists(_dbFileName))
            {
                using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
                {
                    conn.Open();

                    string query = "CREATE TABLE data (\r\n    id           INT,\r\n    claim_expire DATETIME,\r\n    stake_expire DATETIME,\r\n    address      TEXT,\r\n    tokens       DECIMAL\r\n,\r\n    chain       TEXT\r\n, UNIQUE(id, chain));";

                    using (SqliteCommand comm = new SqliteCommand(query, conn))
                    {
                        comm.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateClaimInDB(int accountId, DateTime claimExpire, string address, string chain)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"   INSERT INTO data (id, claim_expire, address, chain)
                                        VALUES(@id, @claim_expire, @address, @chain) 
                                        ON CONFLICT(id, chain) 
                                        DO UPDATE SET claim_expire = @claim_expire, address = @address, chain = @chain;";
                //string statement = "INSERT OR REPLACE INTO data (id, claim_expire, address, chain) VALUES (@id, @claim_expire, @address, @chain)";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", accountId);
                    command.Parameters.AddWithValue("@claim_expire", claimExpire);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@chain", chain);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
