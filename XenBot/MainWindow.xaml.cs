﻿using Nethereum.HdWallet;
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
using System.Windows.Markup;

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
        private int _wallets = 1;
        private Wallet _wallet;
        private bool cancelPressed = false;
        private long _globalRank = 1;
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
            _dataController.AccountAdded += _dataController_AccountAdded;
            _dataController.AccountUpdated += _dataController_AccountUpdated;
            _dataController.AccountDeleted += _dataController_AccountDeleted;
            AccountsGrid.ItemsSource = _accountsDG;
            CancelBtn.IsEnabled = false;

            try
            {
                EnableApp(false);
                LoadWallet();
                await LoadInfo();
                Setup();
                RefreshGrid();
                LoadTotals();
            }
            finally
            {
                EnableApp(true);
            }

            
        }

        

        private void LoadTotals()
        {
            var totals = _dataController.AggregateAccountsByChain();

            ETHTOT.Text = totals.ContainsKey("ETH") ? totals["ETH"].ToString() : "0";
            BSCTOT.Text = totals.ContainsKey("BSC") ? totals["BSC"].ToString() : "0";
            MATICTOT.Text = totals.ContainsKey("MATIC") ? totals["MATIC"].ToString() : "0";
        }

        private async Task LoadInfo()
        {
            await GetPrice();
            await LoadBalance();
            await LoadMaxCostInDollars();
            await LoadCurrentMaxTerm();
            _globalRank = await _xenBlockchainController.GetGlobalRank();
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
            else if (cbBlockChain.SelectedIndex == 2)
            {
                _blockchainController = _blockchainControllerFactory.CreateMaticBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenMaticBlockchainController();
                _webController = _webControllerFactory.CreateMaticWebController();
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
            string price = string.Format("{0:N4}", _price * claimRankTransactionFee);
            MaxGasCost.Text = price;
        }

        private void RefreshGrid()
        {
            _accountsDG.Clear();

            List<Entities.Account> accounts = _dataController.GetAccountsByChain(cbBlockChain.Text);
            
            for(int x = 0; x < accounts.Count; x++)
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

                int walletsCreated = 0;

                while(true)
                {
                    if(cancelPressed == true)
                    {
                        return;
                    }

                    var accountfromDB = _dataController.GetAccountByIdAndChain(accountId, _blockchainController.ChainName);

                    if (accountfromDB == null)
                    {
                        Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(accountId).PrivateKey);
                        var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                        //Does not have a rank
                        if (HasClaimedRank(accountId, userMintResult) == false)
                        {
                            GasPrice gasPrice;
                            BigInteger claimRankGas = 0;
                            BigInteger claimRankTransactionFee = 0;

                            while (true)
                            {
                                gasPrice = await _blockchainController.EstimateGasPriceAsync(_priorityFee);
                                claimRankGas = await _xenBlockchainController.EstimateGasToClaim(mintAccount.Address, termDays);
                                claimRankTransactionFee = gasPrice.Priority * claimRankGas;

                                var maxFeeWillingToPay = Web3.Convert.ToWei((decimal.Parse(MaxGasCost.Text) / _price));

                                if (maxFeeWillingToPay > claimRankTransactionFee)
                                {
                                    break;
                                }

                                await Task.Delay(3500);

                                if (cancelPressed == true)
                                {
                                    return;
                                }
                            }

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
                                MessageBox.Show("Done - ran out of money");
                                break;
                            }

                            var mintAccountBalance = await _blockchainController.Getbalance(mintAccount.Address);
                            BigInteger amountToSend = mintAccountBalance >= claimRankTransactionFee ? new BigInteger(0) : claimRankTransactionFee - mintAccountBalance;

                            await SendMoneyToMintAccount(accountId, _wallet, amountToSend);
                            await _xenBlockchainController.ClaimRank(_wallet.GetAccount(accountId), termDays, claimRankGas, gasPrice);
                            
                            walletsCreated++;

                            DateTime claimExpire = DateTime.UtcNow.AddDays(termDays);
                            string address = _wallet.GetAccount(accountId).Address;
                            var data = await _xenBlockchainController.GetUserMints(address);
                            var tokens = _xenBlockchainController.GetGrossReward(_globalRank, (long)data.Amplifier, (long)data.Term, (long)data.EaaRate, (long)data.Rank);
                            _dataController.UpdateClaimInDB(accountId, claimExpire, address, chain, (long)data.Rank, (long)data.Amplifier, (long)data.EaaRate, (long)data.Term, tokens);
                        }
                        else
                        {
                            var term = userMintResult.Term;
                            var matureDate = UnixTimeStampToDateTime((long)userMintResult.MaturityTs);
                            var address = userMintResult.Address;
                            var tokens = _xenBlockchainController.GetGrossReward(_globalRank, (long)userMintResult.Amplifier, (long)userMintResult.Term, (long)userMintResult.EaaRate, (long)userMintResult.Rank);
                            _dataController.UpdateClaimInDB(accountId, UnixTimeStampToDateTime((long)userMintResult.MaturityTs), address, chain, (long)userMintResult.Rank, (long)userMintResult.Amplifier, (long)userMintResult.EaaRate, (long)userMintResult.Term, tokens);
                        }
                        LoadTotals();

                        if (_wallets >= walletsCreated)
                        {
                            break;
                        }
                    }

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

        private void _dataController_AccountAdded(object? sender, AccountEventArg e)
        {
            AddAccountToGrid(e.Account);
        }

        private void _dataController_AccountUpdated(object? sender, AccountEventArg e)
        {
            UpdateAccountToGrid(e.Account);
        }

        private void _dataController_AccountDeleted(object? sender, AccountEventArg e)
        {
            
        }

        private void UpdateAccountToGrid(Entities.Account account)
        {
            AccountDG dgObj = _accountsDG.FirstOrDefault(x => x.AccountId == account.AccountId && x.Chain == account.Chain);

            if(dgObj != null)
            {
                dgObj.Address = account.Address;
                dgObj.AccountId = account.AccountId;
                dgObj.Chain = account.Chain;
                dgObj.ClaimExpire = account.ClaimExpire;
                dgObj.DaysLeft = account.DaysLeft;
            }
            else
            {
                AddAccountToGrid(account);
            }
        }

        private void AddAccountToGrid(Entities.Account account)
        {
            AccountDG dgObj = new AccountDG();
            dgObj.Address = account.Address;
            dgObj.AccountId = account.AccountId;
            dgObj.Chain = account.Chain;
            dgObj.ClaimExpire = account.ClaimExpire;
            dgObj.DaysLeft = account.DaysLeft;
            _accountsDG.Add(dgObj);
        }

        private void EnableApp(bool isEnabled)
        {
            UpdatePriceButton.IsEnabled = isEnabled;
            MaxGasCost.IsEnabled = isEnabled;
            tbWallets.IsEnabled = isEnabled;
            Address.IsEnabled = isEnabled;
            TermDays.IsEnabled = isEnabled;
            ClaimRankBtn.IsEnabled = isEnabled;
            cbBlockChain.IsEnabled = isEnabled;
            btnGetExistingAccounts.IsEnabled = isEnabled;
            cbPriorityFee.IsEnabled = isEnabled;
            ShowPrivateKey.IsEnabled = isEnabled;
            UpdatePriceButton.IsEnabled = isEnabled;
        }

        private bool HasClaimedRank(int accountId, UserMintOutputDTO userMintResult)
        {
            return userMintResult.Address == _wallet.GetAccount(accountId).Address;
        }

        //private async Task ClaimRank(int accountId, Wallet wallet, BigInteger gas, int termDays)
        //{
        //    await _xenBlockchainController.ClaimRank(wallet.GetAccount(accountId), termDays, _priorityFee);
        //}

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

        private void cbPriorityFee_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _priorityFee = int.Parse(cbPriorityFee.Text);
            }
        }

        private async void cbBlockChain_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                try
                {
                    EnableApp(false);
                    LoadFactories();
                    RefreshGrid();
                    await LoadInfo();
                }
                finally
                {
                    EnableApp(true);
                }
            }
        }

        private async void btnGetExistingAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cc  =_wallet.GetAccount(int.MaxValue);
                int accountId = 0;

                string chain = _blockchainController.ChainName;

                Nethereum.Web3.Accounts.Account mainAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(0).PrivateKey);

                int notMintedCount = 0;

                while (true)
                {
                    var accountfromDB = _dataController.GetAccountByIdAndChain(accountId, chain);

                    if (accountfromDB == null)
                    {
                        Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(accountId).PrivateKey);
                        var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                        //Does not have a rank
                        if (HasClaimedRank(accountId, userMintResult) == false)
                        {
                            notMintedCount++;
                        }
                        else
                        {
                            notMintedCount = 0;
                            var term = userMintResult.Term;
                            var matureDate = UnixTimeStampToDateTime((long)userMintResult.MaturityTs);
                            var address = userMintResult.Address;
                            var tokens = _xenBlockchainController.GetGrossReward(_globalRank, (long)userMintResult.Amplifier, (long)userMintResult.Term, (long)userMintResult.EaaRate, (long)userMintResult.Rank);
                            _dataController.UpdateClaimInDB(accountId, matureDate, address, chain, (long)userMintResult.Rank, (long)userMintResult.Amplifier, (long)userMintResult.EaaRate, (long)userMintResult.Term, tokens);
                            LoadTotals();
                        }
                    }
                    else
                    {
                        notMintedCount = 0;

                        if (accountfromDB.ClaimExpire != null && DateTime.UtcNow > accountfromDB.ClaimExpire) //is expired
                        {
                            //var xx = _wallet.GetAccount(accountId).PrivateKey;
                            Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(accountId).PrivateKey);
                            var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                            if(userMintResult.Address != mintAccount.Address) // It's claimed. So remove in local database
                            {
                                _dataController.DeleteClaimByIdAndChain(accountId, chain);
                            }
                        }
                    }

                    if (notMintedCount >= 5)
                    {
                        break;
                    }

                    accountId++;
                }

                RefreshGrid();
                LoadTokens();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadTokens()
        {
            var dict = _dataController.AggregateTokensByChain();
            MessageBox.Show(dict["MATIC"].ToString());
        }

        private void tbWallets_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(tbWallets.Text, out _wallets) == false)
            {
                _wallets = 1;
                MessageBox.Show("Invalid number of wallets");
                tbWallets.Text = "1";
            }
        }

        private async void btnRefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnableApp(false);
                await LoadInfo();
                _globalRank = await _xenBlockchainController.GetGlobalRank();
            }
            finally
            {
                EnableApp(true);
            }
        }
    }
}
