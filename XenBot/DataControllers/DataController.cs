using Microsoft.Data.Sqlite;
using Nethereum.Signer;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using System.Xml;
using XenBot.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace XenBot.DataControllers
{
    public class DataController
    {
        private string _dbFileName;

        public event EventHandler<AccountEventArg> AccountAdded;
        public event EventHandler<AccountEventArg> AccountUpdated;
        public event EventHandler<AccountEventArg> AccountDeleted;

        protected virtual void OnAccountAdded(Claim account)
        {
            AccountAdded?.Invoke(this, new AccountEventArg(account));
        }

        protected virtual void OnAccountUpdated(Claim account)
        {
            AccountUpdated?.Invoke(this, new AccountEventArg(account));
        }

        protected virtual void OnAccountDeleted(Claim account)
        {
            AccountUpdated?.Invoke(this, new AccountEventArg(account));
        }

        public DataController()
        {
            _dbFileName = "data.db";
        }

        public List<Entities.Account> GetAllAccounts()
        {
            List<Entities.Account> accounts = new List<Entities.Account>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, name, address from accounts order by name";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int nameOrdinal = reader.GetOrdinal("name");
                        int addressOrdinal = reader.GetOrdinal("address");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.Id = reader.GetInt32(idOrdinal);
                            account.Name = reader.GetString(nameOrdinal);
                            account.Address = reader.GetString(addressOrdinal);
                            accounts.Add(account);
                        }
                    }
                }
            }

            return accounts;
        }

        public List<Entities.Account> GetAllAccountsWithPhrase()
        {
            List<Entities.Account> accounts = new List<Entities.Account>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, name, address, phrase from accounts order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int nameOrdinal = reader.GetOrdinal("name");
                        int addressOrdinal = reader.GetOrdinal("address");
                        int phraseOrdinal = reader.GetOrdinal("phrase");

                        while (reader.Read())
                        {
                            Entities.Account account = new Entities.Account();
                            account.Id = reader.GetInt32(idOrdinal);
                            account.Name = reader.GetString(nameOrdinal);
                            account.Address = reader.GetString(addressOrdinal);
                            account.Phrase = reader.GetString(phraseOrdinal);
                            accounts.Add(account);
                        }
                    }
                }
            }

            return accounts;
        }

        public Entities.Account GetAccountWithPhrase(int accountId)
        {
            Entities.Account account = null;

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, name, phrase, address from accounts where id = @account_id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@account_id", accountId);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int nameOrdinal = reader.GetOrdinal("name");
                        int phraseOrdinal = reader.GetOrdinal("phrase");
                        int addressOrdinal = reader.GetOrdinal("address");

                        if (reader.Read())
                        {
                            account = new Account();
                            account.Id = reader.GetInt32(idOrdinal);
                            account.Name = reader.GetString(nameOrdinal);
                            account.Phrase = reader.GetString(phraseOrdinal);
                            account.Address = reader.GetString(addressOrdinal);
                        }
                        else
                        {
                            throw new Exception("Could not find account");
                        }
                    }
                }
            }

            return account;
        }

        public void AddAccount(Entities.Account account)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"INSERT INTO accounts (name, phrase, address) VALUES (@name, @phrase, @address);";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@name", account.Name);
                    command.Parameters.AddWithValue("@phrase", account.Phrase);
                    command.Parameters.AddWithValue("@address", account.Address);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ChangePassword(List<Entities.Account> accounts)
        {
            using(TransactionScope scope = new TransactionScope())
            {
                foreach(var account in accounts)
                {
                    UpdateAccountPhrase(account.Id, account.Phrase);
                }

                scope.Complete();
            }
        }

        private void UpdateAccountPhrase(int id, string phrase)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"UPDATE accounts SET phrase = @phrase WHERE id = @id;";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@phrase", phrase);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAccount(int accountId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                DeleteClaimsByAccount(accountId);

                using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
                {
                    conn.Open();

                    string statement = @"DELETE FROM accounts WHERE id = @account_id";

                    using (SqliteCommand command = new SqliteCommand(statement, conn))
                    {
                        command.Parameters.AddWithValue("@account_id", accountId);
                        command.ExecuteNonQuery();
                    }
                }

                scope.Complete();
            }
        }

        public void DeleteClaimsByAccount(int accountId)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"DELETE FROM data WHERE account_id = @account_id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@account_id", accountId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Entities.Claim> GetExpiredClaimsByChain(string chain, int accountId)
        {
            List<Entities.Claim> accounts = new List<Entities.Claim>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where claim_expire <= @claim_expire and chain = @chain and account_id = @account_id order by claim_expire";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {

                    command.Parameters.AddWithValue("@claim_expire", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@account_id", accountId);
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
                            Entities.Claim account = new Entities.Claim();
                            account.Id = reader.GetInt32(idOrdinal);
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

        public List<Entities.Claim> GetClaimsByChain(string chain, int accountId)
        {
            List<Entities.Claim> accounts = new List<Entities.Claim>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where chain = @chain and account_id = @account_id order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@account_id", accountId);
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
                            Entities.Claim account = new Entities.Claim();
                            account.Id = reader.GetInt32(idOrdinal);
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

        public Entities.Claim? GetClaimByIdAndChain(int claimId, string chain, int accountId)
        {
            Entities.Claim account = null;

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select id, claim_expire, stake_expire, address, chain, rank, amplifier, eaa_rate, term, tokens from data where id = @id and chain = @chain and account_id = @account_id order by id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", claimId);
                    command.Parameters.AddWithValue("@account_id", accountId);
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
                            account = new Entities.Claim();
                            account.Id = reader.GetInt32(idOrdinal);
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

        public Dictionary<string, long> AggregateTokensByChain(int accountId)
        {
            Dictionary<string, long> chainDict = new Dictionary<string, long>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select chain, sum(tokens) as tot from data where account_id = @account_id group by chain";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@account_id", accountId);

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

        public Dictionary<string, int> AggregateClaimsByChain(int accountId)
        {
            Dictionary<string, int> chainDict = new Dictionary<string, int>();

            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = "select chain, count(*) as count from data where account_id = @account_id group by chain";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@account_id", accountId);

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
                                        account_id   INT NOT NULL,
	                                    claim_expire DATETIME,
	                                    stake_expire DATETIME,
	                                    address      TEXT,
	                                    tokens       LONG INTEGER,
	                                    chain        TEXT, 
	                                    rank         integer,
                                        term         integer,
	                                    amplifier     integer,
	                                    eaa_rate     integer,
                                        FOREIGN KEY (account_id) REFERENCES accounts (id),
	                                    UNIQUE(id, chain, account_id));

                                        CREATE TABLE accounts (
	                                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
	                                    name         TEXT NOT NULL UNIQUE,
                                        phrase       TEXT NOT NULL,
                                        address      TEXT NOT NULL UNIQUE);";

                    using (SqliteCommand comm = new SqliteCommand(query, conn))
                    {
                        comm.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteClaimByIdAndChain(int claimId, string chain, int accountId)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"DELETE FROM data WHERE id = @id and chain = @chain and account_id = @account_id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", claimId);
                    command.Parameters.AddWithValue("@chain", chain);
                    command.Parameters.AddWithValue("@account_id", accountId);
                    command.ExecuteNonQuery();
                }
            }

            OnAccountDeleted(new Claim() { Id = claimId, Chain = chain });
        }

        public void UpdateTokensByIdAndChain(int claimId, int accountId, string chain, long tokens)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"UPDATE data SET tokens = @tokens where id = @id and chain = @chain and account_id = @account_id";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", claimId);
                    command.Parameters.AddWithValue("@tokens", tokens);
                    command.Parameters.AddWithValue("@chain", chain);
                    command.Parameters.AddWithValue("@account_id", accountId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public long? UpdateClaimInDB(int claimId, int accountId, DateTime claimExpire, string address, string chain, long rank, long amplifier, long eaaRate, long term, long tokens)
        {
            using (SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};", _dbFileName)))
            {
                conn.Open();

                string statement = @"   INSERT INTO data (id, claim_expire, address, chain, rank, amplifier, eaa_rate, term, tokens, account_id)
                                        VALUES(@id, @claim_expire, @address, @chain, @rank, @amplifier, @eaa_rate, @term, @tokens, @account_id) 
                                        ON CONFLICT(id, chain, account_id) 
                                        DO UPDATE SET claim_expire = @claim_expire, address = @address, chain = @chain, rank = @rank, amplifier = @amplifier, eaa_rate = @eaa_rate, term = @term, tokens = @tokens, account_id = @account_id;
                                        SELECT last_insert_rowid();";

                using (SqliteCommand command = new SqliteCommand(statement, conn))
                {
                    command.Parameters.AddWithValue("@id", claimId);
                    command.Parameters.AddWithValue("@account_id", accountId);
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
                        Claim account = new Claim();
                        account.Id = claimId;
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
                        Claim account = new Claim();
                        account.Id = claimId;
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
