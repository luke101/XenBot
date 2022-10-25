using Microsoft.Data.Sqlite;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenBot.Entities;

namespace XenBot.DataControllers
{
    public class DataController
    {
        private string _dbFileName;

        public event EventHandler<AccountEventArg> AccountAdded;
        public event EventHandler<AccountEventArg> AccountUpdated;
        public event EventHandler<AccountEventArg> AccountDeleted;

        protected virtual void OnAccountAdded(Account account)
        {
            AccountAdded?.Invoke(this, new AccountEventArg(account));
        }

        protected virtual void OnAccountUpdated(Account account)
        {
            AccountUpdated?.Invoke(this, new AccountEventArg(account));
        }

        protected virtual void OnAccountDeleted(Account account)
        {
            AccountUpdated?.Invoke(this, new AccountEventArg(account));
        }

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

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
                        int stakeExpireOrdinal = reader.GetOrdinal("stake_expire");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int rankOrdinal = reader.GetOrdinal("rank");
                        int amplifierOrdinal = reader.GetOrdinal("amplifier");
                        int eaarateOrdinal = reader.GetOrdinal("eaa_rate");
                        int termOrdinal = reader.GetOrdinal("term");
                        int tokensOrdinal = reader.GetOrdinal("tokens");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.AccountId = reader.GetInt32(idOrdinal);
                            account.ClaimExpire = reader.IsDBNull(claimExpireOrdinal) ? null : reader.GetDateTime(claimExpireOrdinal).ToLocalTime();
                            account.StakeExpire = reader.IsDBNull(stakeExpireOrdinal) ? null : reader.GetDateTime(stakeExpireOrdinal).ToLocalTime();
                            account.Address = reader.GetString(addressOrdinal);
                            account.Chain = reader.GetString(chainOrdinal);
                            account.Rank = reader.GetInt64(rankOrdinal);
                            account.Amplifier = reader.GetInt64(amplifierOrdinal);
                            account.EaaRate = reader.GetInt64(eaarateOrdinal);
                            account.Term = reader.GetInt64(termOrdinal);
                            account.Tokens = reader.GetInt64(tokensOrdinal);
                            accounts.Add(account);
                        }

                    }
                }
            }

            return accounts;
        }

        public List<Entities.Account> GetExpiredAccountsByChain(string chain)
        {
            List<Entities.Account> accounts = new List<Entities.Account>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where claim_expire <= @claim_expire and chain = @chain order by claim_expire";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {

                    command.Parameters.AddWithValue("@claim_expire", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@chain", chain);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
                        int stakeExpireOrdinal = reader.GetOrdinal("stake_expire");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int rankOrdinal = reader.GetOrdinal("rank");
                        int amplifierOrdinal = reader.GetOrdinal("amplifier");
                        int eaarateOrdinal = reader.GetOrdinal("eaa_rate");
                        int termOrdinal = reader.GetOrdinal("term");
                        int tokensOrdinal = reader.GetOrdinal("tokens");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.AccountId = reader.GetInt32(idOrdinal);
                            account.ClaimExpire = reader.IsDBNull(claimExpireOrdinal) ? null : reader.GetDateTime(claimExpireOrdinal).ToLocalTime();
                            account.StakeExpire = reader.IsDBNull(stakeExpireOrdinal) ? null : reader.GetDateTime(stakeExpireOrdinal).ToLocalTime();
                            account.Address = reader.GetString(addressOrdinal);
                            account.Chain = reader.GetString(chainOrdinal);
                            account.Rank = reader.GetInt64(rankOrdinal);
                            account.Amplifier = reader.GetInt64(amplifierOrdinal);
                            account.EaaRate = reader.GetInt64(eaarateOrdinal);
                            account.Term = reader.GetInt64(termOrdinal);
                            account.Tokens = reader.GetInt64(tokensOrdinal);
                            accounts.Add(account);
                        }

                    }
                }
            }

            return accounts;
        }

        public List<Entities.Account> GetAccountsByChain(string chain)
        {
            List<Entities.Account> accounts = new List<Entities.Account>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where chain = @chain order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@chain", chain);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
                        int stakeExpireOrdinal = reader.GetOrdinal("stake_expire");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int rankOrdinal = reader.GetOrdinal("rank");
                        int amplifierOrdinal = reader.GetOrdinal("amplifier");
                        int eaarateOrdinal = reader.GetOrdinal("eaa_rate");
                        int termOrdinal = reader.GetOrdinal("term");
                        int tokensOrdinal = reader.GetOrdinal("tokens");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.AccountId = reader.GetInt32(idOrdinal);
                            account.ClaimExpire = reader.IsDBNull(claimExpireOrdinal) ? null : reader.GetDateTime(claimExpireOrdinal).ToLocalTime();
                            account.StakeExpire = reader.IsDBNull(stakeExpireOrdinal) ? null : reader.GetDateTime(stakeExpireOrdinal).ToLocalTime();
                            account.Address = reader.GetString(addressOrdinal);
                            account.Chain = reader.GetString(chainOrdinal);
                            account.Rank = reader.GetInt64(rankOrdinal);
                            account.Amplifier = reader.GetInt64(amplifierOrdinal);
                            account.EaaRate = reader.GetInt64(eaarateOrdinal);
                            account.Term = reader.GetInt64(termOrdinal);
                            account.Tokens = reader.GetInt64(tokensOrdinal);
                            accounts.Add(account);
                        }

                    }
                }
            }

            return accounts;
        }

        public Entities.Account? GetAccountByIdAndChain(int id, string chain)
        {
            Entities.Account account = null;

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where id = @id and chain = @chain order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@chain", chain);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int claimExpireOrdinal = reader.GetOrdinal("claim_expire");
                        int stakeExpireOrdinal = reader.GetOrdinal("stake_expire");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int rankOrdinal = reader.GetOrdinal("rank");
                        int amplifierOrdinal = reader.GetOrdinal("amplifier");
                        int eaarateOrdinal = reader.GetOrdinal("eaa_rate");
                        int termOrdinal = reader.GetOrdinal("term");
                        int tokensOrdinal = reader.GetOrdinal("tokens");

                        if (reader.Read())
                        {
                            account = new Entities.Account();
                            account.AccountId = reader.GetInt32(idOrdinal);
                            account.ClaimExpire = reader.IsDBNull(claimExpireOrdinal) ? null : reader.GetDateTime(claimExpireOrdinal).ToLocalTime();
                            account.StakeExpire = reader.IsDBNull(stakeExpireOrdinal) ? null : reader.GetDateTime(stakeExpireOrdinal).ToLocalTime();
                            account.Address = reader.GetString(addressOrdinal);
                            account.Chain = reader.GetString(chainOrdinal);
                            account.Rank = reader.GetInt64(rankOrdinal);
                            account.Amplifier = reader.GetInt64(amplifierOrdinal);
                            account.EaaRate = reader.GetInt64(eaarateOrdinal);
                            account.Term = reader.GetInt64(termOrdinal);
                            account.Tokens = reader.GetInt64(tokensOrdinal);
                        }

                    }
                }
            }

            return account;
        }

        public Dictionary<string, long> AggregateTokensByChain()
        {
            Dictionary<string, long> chainDict = new Dictionary<string, long>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select chain, sum(tokens) as tot from data group by chain";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int chainOrdinal = reader.GetOrdinal("chain");
                        int countOrdinal = reader.GetOrdinal("tot");

                        while (reader.Read())
                        {
                            chainDict[reader.GetString(chainOrdinal)] = reader.GetInt64(countOrdinal);
                        }
                    }
                }
            }

            return chainDict;
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

                    string query = @"CREATE TABLE data (
	                                    id           INT,
	                                    claim_expire DATETIME,
	                                    stake_expire DATETIME,
	                                    address      TEXT,
	                                    tokens       LONG INTEGER,
	                                    chain        TEXT, 
	                                    rank         integer,
                                        term         integer,
	                                    amplifier     integer,
	                                    eaa_rate     integer,
	                                    UNIQUE(id, chain));";

                    using (SqliteCommand comm = new SqliteCommand(query, conn))
                    {
                        comm.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteClaimByIdAndChain(int accountId, string chain)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"DELETE FROM data WHERE id = @id and chain = @chain";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", accountId);
                    command.Parameters.AddWithValue("@chain", chain);
                    command.ExecuteNonQuery();
                }
            }

            OnAccountDeleted(new Account() { AccountId = accountId, Chain = chain });
        }

        public long? UpdateClaimInDB(int accountId, DateTime claimExpire, string address, string chain, long rank, long amplifier, long eaaRate, long term, long tokens)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"   INSERT INTO data (id, claim_expire, address, chain, rank, amplifier, eaa_rate, term, tokens)
                                        VALUES(@id, @claim_expire, @address, @chain, @rank, @amplifier, @eaa_rate, @term, @tokens) 
                                        ON CONFLICT(id, chain) 
                                        DO UPDATE SET claim_expire = @claim_expire, address = @address, chain = @chain, rank = @rank, amplifier = @amplifier, eaa_rate = @eaa_rate, term = @term, tokens = @tokens;
                                        SELECT last_insert_rowid();";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", accountId);
                    command.Parameters.AddWithValue("@claim_expire", claimExpire);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@chain", chain);
                    command.Parameters.AddWithValue("@rank", rank);
                    command.Parameters.AddWithValue("@amplifier", amplifier);
                    command.Parameters.AddWithValue("@eaa_rate", eaaRate);
                    command.Parameters.AddWithValue("@term", term);
                    command.Parameters.AddWithValue("@tokens", tokens);
                    long? id = (long?)command.ExecuteScalar();

                    if(id == 0)
                    {
                        Account account = new Account();
                        account.AccountId = accountId;
                        account.Address = address;
                        account.Chain = chain;
                        account.ClaimExpire = claimExpire;
                        account.Rank = rank;
                        account.EaaRate = eaaRate;
                        account.Term = term;
                        account.Amplifier = amplifier;
                        account.Tokens = tokens;
                        OnAccountUpdated(account);
                    }
                    else
                    {
                        Account account = new Account();
                        account.AccountId = accountId;
                        account.Address = address;
                        account.Chain = chain;
                        account.ClaimExpire = claimExpire;
                        account.Rank = rank;
                        account.EaaRate = eaaRate;
                        account.Term = term;
                        account.Amplifier = amplifier;
                        account.Tokens = tokens;
                        OnAccountAdded(account);
                    }

                    return id;
                }
            }
        }
    }
}
