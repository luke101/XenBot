using Nethereum.HdWallet;
using Nethereum.RPC.Eth;
using Nethereum.Web3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nethereum.Contracts.Extensions;
using Org.BouncyCastle.Utilities;
using RestSharp;
using Rijndael256;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nethereum.RPC.Eth.DTOs;
using XenBot.InputDTOs;
using Nethereum.Model;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Org.BouncyCastle.Cms;
using System.Numerics;
using Nethereum.Contracts;
using System.Threading;
using System.Security.Cryptography.Xml;
using Microsoft.Data.Sqlite;
using XenBot.Entities;
using System.Collections.ObjectModel;
using XenBot.DatagridEntities;
using Newtonsoft.Json.Converters;
using Nethereum.RPC.Accounts;
using System.Security.Principal;
using XenBot.BlockChainControllers;
using XenBot.Factories;
using XenBot.DataControllers;
using XenBot.WebControllers;

namespace XenBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _password;
        private decimal _price = 0;
        private int _priorityFee = 1;
        private Wallet _wallet;
        private bool cancelPressed = false;
        private List<Entities.Account> _accounts = new List<Entities.Account>();
        
        private BlockchainControllerFactory _blockchainControllerFactory;
        private XenBlockchainControllerFactory _xenBlockchainControllerFactory;
        private WebControllerFactory _webControllerFactory;
        private IBlockchainController _blockchainController;
        private IXenBlockChainController _xenBlockchainController;
        private IWebController _webController;
        private DataController _dataController;

        public ObservableCollection<AccountDG> _accountsDG = new ObservableCollection<AccountDG>();

        public MainWindow(string password)
        {
            InitializeComponent();
            Loaded += MainLoaded;
            _password = password;
            _blockchainControllerFactory = new BlockchainControllerFactory();
            _xenBlockchainControllerFactory = new XenBlockchainControllerFactory(_blockchainControllerFactory);
            _webControllerFactory = new WebControllerFactory();
            _dataController = new DataController();
        }

        private async void MainLoaded(object sender, RoutedEventArgs e)
        {
            LoadFactories();

            AccountsGrid.ItemsSource = _accountsDG;
            InfuraUrl.Text = "https://bsc-dataseed.binance.org/";
            LoadWallet();
            await LoadInfo();
            Setup();
            RefreshGrid();
            RefreshTotal();
            CancelBtn.IsEnabled = false;
        }

        private async Task LoadInfo()
        {
            await GetPrice();
            await LoadBalance();
            await LoadMaxCostInDollars();
            await LoadCurrentMaxTerm();
        }

        private void LoadFactories()
        {
            if (cbBlockChain.SelectedIndex == 1)
            {
                _blockchainController = _blockchainControllerFactory.CreateBscBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenBscBlockchainController();
                _webController = _webControllerFactory.CreateBscWebController();
            }
            else if(cbBlockChain.SelectedIndex == 0)
            {
                _blockchainController = _blockchainControllerFactory.CreateEthBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenEthBlockchainController();
                _webController = _webControllerFactory.CreateEthWebController();
            }
        }

        private async Task LoadCurrentMaxTerm()
        {
            var term = await _xenBlockchainController.GetCurrentMaxTerm();
            TermDays.Text = term.ToString();
        }

        private async Task LoadMaxCostInDollars()
        {
            MaxGasCost.Text = "";
            GasPrice gasPrice = await _blockchainController.EstimateGasPriceAsync(_priorityFee);
            BigInteger gas = await _xenBlockchainController.EstimateGasToClaim(_wallet.GetAccount(int.MaxValue).Address, 1);
            decimal claimRankTransactionFee = Web3.Convert.FromWei(gasPrice.Price * gas);
            string price = string.Format("{0:N}", _price * claimRankTransactionFee);
            MaxGasCost.Text = price;
        }

        private void RefreshTotal()
        {
            var total = _dataController.GetTotalClaims();

            TotalMints.Text = total.ToString();
        }

        private void RefreshGrid()
        {
            AccountsGrid.Items.Refresh();

            List<Entities.Account> accounts = _dataController.GetAccounts();

            int accountDGCount = _accountsDG.Count;

            for(int x = 0; x < accounts.Count; x++)
            {
                if(x + 1 > accountDGCount)
                {
                    AccountDG accountDG = new AccountDG();
                    accountDG.AccountId = accounts[x].AccountId;
                    accountDG.ClaimExpire = accounts[x].ClaimExpire;
                    accountDG.StakeExpire = accounts[x].StakeExpire;
                    accountDG.Address = accounts[x].Address;
                    accountDG.DaysLeft = accounts[x].DaysLeft;
                    accountDG.Chain = accounts[x].Chain;
                    _accountsDG.Add(accountDG);
                }
                else
                {
                    _accountsDG[x].AccountId = accounts[x].AccountId;
                    _accountsDG[x].ClaimExpire = accounts[x].ClaimExpire;
                    _accountsDG[x].StakeExpire = accounts[x].StakeExpire;
                    _accountsDG[x].Address = accounts[x].Address;
                    _accountsDG[x].DaysLeft = accounts[x].DaysLeft;
                    _accountsDG[x].Chain = accounts[x].Chain;
                }
            }
        }

        private void Setup()
        {
            _dataController.CreateDFile();
        }

        private void LoadWallet()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletPath = System.IO.Path.Combine(currentDirectory, "wallet.data");

            string words = string.Empty;

            try
            {
                string encryptedWords = File.ReadAllText(walletPath);
                words = Rijndael.Decrypt(encryptedWords, _password, KeySize.Aes256);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            _wallet = new Wallet(words, null);
            Address.Text = _wallet.GetAccount(0).Address;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await LoadBalance();
        }

        private async Task LoadBalance()
        {
            var balance = await _blockchainController.Getbalance(_wallet.GetAccount(0).Address);
            Balance.Text = Web3.Convert.FromWei(balance).ToString();
        }

        private async Task GetPrice()
        {
            _price = await _webController.GetPriceAsync();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                CancelBtn.IsEnabled = true;
                EnableApp(false);

                int accountId = 0;

                bool done = false;

                int termDays = 0;

                string chain = _blockchainController.ChainName;

                if (string.IsNullOrWhiteSpace(TermDays.Text) || int.TryParse(TermDays.Text, out termDays) == false)
                {
                    MessageBox.Show("Invalid term days");
                    return;
                }
                else if (termDays <= 0)
                {
                    MessageBox.Show("Invalid term days");
                    return;
                }


                decimal maxClaimRankGasInDollars = 0;

                if (string.IsNullOrWhiteSpace(MaxGasCost.Text) || decimal.TryParse(MaxGasCost.Text, out maxClaimRankGasInDollars) == false)
                {
                    MessageBox.Show("Invalid max gas cost");
                    return;
                }
                else if (termDays <= 0)
                {
                    MessageBox.Show("Invalid max gas cost");
                    return;
                }

                Nethereum.Web3.Accounts.Account mainAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(0).PrivateKey);

                while (!done || accountId <= 2)
                {
                    if(cancelPressed == true)
                    {
                        return;
                    }

                    Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(accountId).PrivateKey);
                    var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                    //Does not have a rank
                    if (HasClaimedRank(accountId, userMintResult) == false)
                    {
                        GasPrice gasPrice = await _blockchainController.EstimateGasPriceAsync(_priorityFee);
                        BigInteger claimRankGas = await _xenBlockchainController.EstimateGasToClaim(mintAccount.Address, termDays);
                        BigInteger claimRankTransactionFee = gasPrice.Priority * claimRankGas;

                        if (cancelPressed == true)
                        {
                            return;
                        }

                        var seedAccountBalance = await _blockchainController.Getbalance(mainAccount.Address);
                        BigInteger transferGas = await _blockchainController.EstimateCoinTransferGas(mainAccount.Address, mintAccount.Address, claimRankGas);
                        var transferFee = gasPrice.Priority * transferGas;
                        var claimFee = gasPrice.Priority * claimRankGas;
                        var transferAmount = transferFee + claimFee;
                        if (seedAccountBalance <= transferAmount)
                        {
                            MessageBox.Show("Done");
                            break;
                        }

                        var mintAccountBalance = await _blockchainController.Getbalance(mintAccount.Address);
                        BigInteger amountToSend = mintAccountBalance >= claimRankTransactionFee ? new BigInteger(0) : claimRankTransactionFee - mintAccountBalance;

                        await SendMoneyToMintAccount(accountId, _wallet, amountToSend);
                        await ClaimRank(accountId, _wallet, claimRankGas, termDays);

                        DateTime claimExpire = DateTime.UtcNow.AddDays(termDays);
                        string address = _wallet.GetAccount(accountId).Address;
                        UpdateClaimInDB(accountId, claimExpire, address, chain);
                    }
                    else
                    {
                        var term = userMintResult.Term;
                        var matureDate = UnixTimeStampToDateTime((long)userMintResult.MaturityTs);
                        var address = userMintResult.Address;
                        UpdateClaimInDB(accountId, matureDate, address, chain);
                    }

                    RefreshGrid();
                    RefreshTotal();
                    accountId++;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cancelPressed = false;
                CancelBtn.IsEnabled = false;
                EnableApp(true);
            }
        }

        private void UpdateClaimInDB(int accountId, DateTime claimExpire, string address, string chain)
        {
            _dataController.UpdateClaimInDB(accountId, claimExpire, address, chain);
        }

        //private void UpdateAccountBalanceInDB(int accountId, decimal amountToSend)
        //{
        //    using(SqliteConnection conn = new SqliteConnection(string.Format("Data Source={0};Version=3;", dbFileName)))
        //    {
        //        conn.Open();

        //        string statement = "insert or replace into data (id, balance) values (select @id, @balance)";

        //        using (SqliteCommand command = new SqliteCommand(statement, conn))
        //        {
        //            command.Parameters.AddWithValue("@id", accountId);
        //            command.Parameters.AddWithValue("@balance", Web3.Convert.FromWei(new BigInteger(amountToSend)));
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        private void EnableApp(bool isEnabled)
        {
            UpdatePriceButton.IsEnabled = isEnabled;
            MaxGasCost.IsEnabled = isEnabled;
            InfuraUrl.IsEnabled = isEnabled;
            Address.IsEnabled = isEnabled;
            TermDays.IsEnabled = isEnabled;
            ClaimRankBtn.IsEnabled = isEnabled;
        }

        private bool HasClaimedRank(int accountId, UserMintOutputDTO userMintResult)
        {
            return userMintResult.Address == _wallet.GetAccount(accountId).Address;
        }

        private async Task ClaimRank(int accountId, Wallet wallet, BigInteger gas, int termDays)
        {
            await _xenBlockchainController.ClaimRank(wallet.GetAccount(accountId), termDays, _priorityFee);
        }

        private async Task<bool> SendMoneyToMintAccount(int accountId, Wallet wallet, BigInteger amountToSend)
        {
            //No need to send money to main account
            if(accountId == 0)
            {
                return true;
            }

            bool success = true;

            if (amountToSend > new BigInteger(0))
            {
                success = await _blockchainController.TransferCoins(wallet.GetAccount(0), _wallet.GetAccount(accountId).Address, amountToSend, _priorityFee);
            }

            return success;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            cancelPressed = !cancelPressed;
            CancelBtn.IsEnabled = false;
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
            return offset.UtcDateTime;
        }

        private void ShowPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Show Private Key", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ShowPrivateKeyWindow showPrivateKeyWindow = new ShowPrivateKeyWindow(_wallet.GetAccount(0).PrivateKey);
                showPrivateKeyWindow.ShowDialog();
            }
        }

        private async void cbBlockChain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                LoadFactories();
                await LoadInfo();
            }
        }

        //private void cbPriorityFee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
            
        //}

        private void cbPriorityFee_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _priorityFee = int.Parse(cbPriorityFee.Text);
            }
        }

        //private void BackUpBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    var accounts = _dataController.GetAccounts();

        //    List<string> lines = new List<string>();

        //    foreach(var account in accounts)
        //    {
        //        string line = string.Format("{0},{1},{2},{3}", account.AccountId, account.ClaimExpire, account.Chain, account.Address);
        //        lines.Add(line);
        //    }

        //    File.WriteAllLines(fileName + ".backup", lines);
        //}
    }
}
