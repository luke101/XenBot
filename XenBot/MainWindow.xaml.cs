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
using XenBot.DTOs;
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
using System.Globalization;
using Microsoft.VisualBasic;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using XenBot.Enums;
using XenBot.Utils;
using System.Net.NetworkInformation;
using System.Security.Claims;

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
        private int _accountId = 0;
        private string _blockChain = string.Empty;
        private List<Entities.Claim> _accounts = new List<Entities.Claim>();
        
        private BlockchainControllerFactory _blockchainControllerFactory;
        private XenBlockchainControllerFactory _xenBlockchainControllerFactory;
        private WebControllerFactory _webControllerFactory;
        private IBlockchainController _blockchainController;
        private IXenBlockChainController _xenBlockchainController;
        private IWebController _webController;
        private DataController _dataController;

        public ObservableCollection<ClaimVM> Claims = new ObservableCollection<ClaimVM>();
        public ObservableCollection<ClaimDueVM> ClaimsDue = new ObservableCollection<ClaimDueVM>();
        public ObservableCollection<AccountVM> Accounts { get; set; }

        public AccountVM SelectedAccount { get; set; }

        public MainWindow(string password)
        {
            InitializeComponent();
            Loaded += MainLoaded;
            _password = password;
            _blockchainControllerFactory = new BlockchainControllerFactory();
            _xenBlockchainControllerFactory = new XenBlockchainControllerFactory(_blockchainControllerFactory);
            _webControllerFactory = new WebControllerFactory();
            _dataController = new DataController();

            Accounts = new ObservableCollection<AccountVM>();
        }

        private async void MainLoaded(object sender, RoutedEventArgs e)
        {
            LoadFactories();
            _dataController.AccountAdded += _dataController_AccountAdded;
            _dataController.AccountUpdated += _dataController_AccountUpdated;
            _dataController.AccountDeleted += _dataController_AccountDeleted;
            AccountsGrid.ItemsSource = Claims;
            ClaimsDueGrid.ItemsSource = ClaimsDue;
            cbAccount.ItemsSource = Accounts;
            CancelBtn.IsEnabled = false;
            _blockChain = cbBlockChain.Text;

            try
            {
                LoadClaimsDue();
                LoadAccounts();
                EnableApp(false);
                LoadWallet();
                await LoadInfo();
                RefreshGrid();
                LoadTotals();
                LoadTokens();
                
            }
            finally
            {
                EnableApp(true);
            }
        }

        private void LoadClaimsDue()
        {
            var claimsDue = _dataController.GetClaimsDueInDays(7);

            Dictionary<string, ClaimDue> claimDict = new Dictionary<string, ClaimDue>();

            foreach (var claim in claimsDue)
            {
                string dueBracket = CalculateDueBracket(claim.ClaimExpire.Value);
                string key = dueBracket + "_" + claim.AccountName + "_" + claim.Chain;

                if (claimDict.ContainsKey(key) == false)
                {
                    var workingClaimDue = new ClaimDue();
                    workingClaimDue.Chain = claim.Chain;
                    workingClaimDue.DueName = dueBracket;
                    workingClaimDue.Account = claim.AccountName;
                    workingClaimDue.Count = 0;
                    workingClaimDue.Tokens = 0;
                    claimDict[key] = workingClaimDue;
                }

                var claimDue = claimDict[key];
                claimDue.Count++;
                claimDue.Tokens = claimDue.Tokens + claim.Tokens;
            }

            ClaimsDue.Clear();

            int claimsDueNow = 0;

            foreach (var key in claimDict.Keys)
            {
                ClaimDueVM claimDue = new ClaimDueVM
                {
                    Tokens = claimDict[key].Tokens.ToString("N0"),
                    Due = claimDict[key].DueName,
                    Chain = claimDict[key].Chain,
                    Account = claimDict[key].Account,
                    Count = claimDict[key].Count.ToString("N0")
                };

                ClaimsDue.Add(claimDue);

                if(claimDue.Due == "Now")
                {
                    claimsDueNow = claimsDueNow + int.Parse(claimDue.Count.Replace(",", ""));
                }
            }

            ClaimsDueTabItem.Header = "Claims Due";

            if(claimsDueNow > 0)
            {
                ClaimsDueTabItem.Header = "Claims Due (" + claimsDueNow + ")";
            }
        }

        private string CalculateDueBracket(DateTime datetime)
        {
            DateTime now = DateTime.Now;

            int days = (datetime - now).Days;

            if(days > 8)
            {
                throw new Exception("Could not calculate due date");
            }

            DateTime day7 = now.AddDays(7);
            DateTime day6 = now.AddDays(6);
            DateTime day5 = now.AddDays(5);
            DateTime day4 = now.AddDays(4);
            DateTime day3 = now.AddDays(3);
            DateTime day2 = now.AddDays(2);
            DateTime tomorrow = now.AddDays(1);

            Tuple<DateTime, DateTime> day7Block = CalculateDayBlock(day7);
            Tuple<DateTime, DateTime> day6Block = CalculateDayBlock(day6);
            Tuple<DateTime, DateTime> day5Block = CalculateDayBlock(day5);
            Tuple<DateTime, DateTime> day4Block = CalculateDayBlock(day4);
            Tuple<DateTime, DateTime> day3Block = CalculateDayBlock(day3);
            Tuple<DateTime, DateTime> day2Block = CalculateDayBlock(day2);
            Tuple<DateTime, DateTime> tomorrowBlock = CalculateDayBlock(tomorrow);

            if (datetime >= day7Block.Item1 && datetime <= day7Block.Item2)
            {
                return "In 7 Days";
            }
            else if(datetime >= day6Block.Item1 && datetime <= day6Block.Item2)
            {
                return "In 6 Days";
            }
            else if (datetime >= day5Block.Item1 && datetime <= day5Block.Item2)
            {
                return "In 5 Days";
            }
            else if (datetime >= day4Block.Item1 && datetime <= day4Block.Item2)
            {
                return "In 4 Days";
            }
            else if (datetime >= day3Block.Item1 && datetime <= day3Block.Item2)
            {
                return "In 3 Days";
            }
            else if (datetime >= day2Block.Item1 && datetime <= day2Block.Item2)
            {
                return "In 2 Days";
            }
            else if (datetime >= tomorrowBlock.Item1 && datetime <= tomorrowBlock.Item2)
            {
                return "Tomorrow";
            }
            else if (now < datetime && now.Day == datetime.Day && now.Year == datetime.Year)
            {
                return "Today";
            }
            else if(now >= datetime)
            {
                return "Now";
            }
            else
            {
                throw new Exception("Could not calculate Claim Due");
            }
        }

        private Tuple<DateTime, DateTime> CalculateDayBlock(DateTime datetime)
        {
            DateTime low = new DateTime(datetime.Year, datetime.Month, datetime.Day, 0, 0, 0);
            DateTime high = new DateTime(datetime.Year, datetime.Month, datetime.Day, 23, 59, 59);

            Tuple<DateTime, DateTime> tuple = new Tuple<DateTime, DateTime>(low, high);

            return tuple;
        }

        private void LoadAccounts()
        {
            Accounts.Clear();

            var accounts = _dataController.GetAllAccounts();

            int counter = 0;
            foreach (var account in accounts) 
            { 
                var accountVM = new AccountVM();
                accountVM.Id = account.Id;
                accountVM.Name = account.Name;
                accountVM.Selected = counter == 0;
                if(counter == 0)
                {
                    _accountId = account.Id;
                    cbAccount.SelectedValue = account.Id.ToString();
                }
                Accounts.Add(accountVM);
                counter++;
            }
        }

        private void LoadTotals()
        {
            var totals = _dataController.AggregateClaimsByChain(_accountId);

            ETHTOT.Text = totals.ContainsKey("ETH") ? totals["ETH"].ToString() : "0";
            BSCTOT.Text = totals.ContainsKey("BSC") ? totals["BSC"].ToString() : "0";
            MATICTOT.Text = totals.ContainsKey("MATIC") ? totals["MATIC"].ToString() : "0";
            FANTOMTOT.Text = totals.ContainsKey("Fantom") ? totals["Fantom"].ToString() : "0";
            ETHWTOT.Text = totals.ContainsKey("EthereumPOW") ? totals["EthereumPOW"].ToString() : "0";
            DCTOT.Text = totals.ContainsKey("Dogechain") ? totals["Dogechain"].ToString() : "0";
            AVAXTOT.Text = totals.ContainsKey("Avalanche") ? totals["Avalanche"].ToString() : "0";
            GLMRTOT.Text = totals.ContainsKey("Moonbeam") ? totals["Moonbeam"].ToString() : "0";
            EVMOSTOT.Text = totals.ContainsKey("Evmos") ? totals["Evmos"].ToString() : "0";
            OKXCHAINTOT.Text = totals.ContainsKey("OKXChain") ? totals["OKXChain"].ToString() : "0";
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
            else if (cbBlockChain.SelectedIndex == 3)
            {
                _blockchainController = _blockchainControllerFactory.CreateFantomBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenFantomBlockchainController();
                _webController = _webControllerFactory.CreateFantomWebController();
            }
            else if (cbBlockChain.SelectedIndex == 4)
            {
                _blockchainController = _blockchainControllerFactory.CreateEthWBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenEthWBlockchainController();
                _webController = _webControllerFactory.CreateEthWWebController();
            }
            else if (cbBlockChain.SelectedIndex == 5)
            {
                _blockchainController = _blockchainControllerFactory.CreateDogechainBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenDogechainBlockchainController();
                _webController = _webControllerFactory.CreateDogechainWebController();
            }
            else if (cbBlockChain.SelectedIndex == 6)
            {
                _blockchainController = _blockchainControllerFactory.CreateAvalancheBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenAvalancheBlockchainController();
                _webController = _webControllerFactory.CreateAvalancheWebController();
            }
            else if (cbBlockChain.SelectedIndex == 7)
            {
                _blockchainController = _blockchainControllerFactory.CreateMoonbeamBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenMoonbeamBlockchainController();
                _webController = _webControllerFactory.CreateMoonbeamWebController();
            }
            else if (cbBlockChain.SelectedIndex == 8)
            {
                _blockchainController = _blockchainControllerFactory.CreateEvmosBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenEvmosBlockchainController();
                _webController = _webControllerFactory.CreateEvmosWebController();
            }
            else if (cbBlockChain.SelectedIndex == 9)
            {
                _blockchainController = _blockchainControllerFactory.CreateOKXChainBlockchainController();
                _xenBlockchainController = _xenBlockchainControllerFactory.CreateXenOKXChainBlockchainController();
                _webController = _webControllerFactory.CreateOKXChainWebController();
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
            Claims.Clear();

            List<Entities.Claim> accounts = _dataController.GetClaimsByChain(cbBlockChain.Text, _accountId);
            
            for(int x = 0; x < accounts.Count; x++)
            {
                ClaimVM accountDG = new ClaimVM();
                accountDG.AccountId = accounts[x].Id;
                accountDG.ClaimExpire = accounts[x].ClaimExpire;
                accountDG.StakeExpire = accounts[x].StakeExpire;
                accountDG.Address = accounts[x].Address;
                accountDG.DaysLeft = accounts[x].DaysLeft;
                accountDG.Chain = accounts[x].Chain;
                accountDG.EstimatedTokens = string.Format(CultureInfo.InvariantCulture, "{0:n0}", accounts[x].Tokens); 
                Claims.Add(accountDG);
            }
        }

        private void LoadWallet()
        {
            var account = _dataController.GetAccountWithPhrase(_accountId);

            string words = string.Empty;

            try
            {
                string encryptedWords = account.Phrase;
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
            Balance.Text = string.Format("{0:N4}", Web3.Convert.FromWei(balance));
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

                int claimId = 0;

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

                if (_wallets <= 0)
                {
                    MessageBox.Show("Invalid number of wallets");
                    return;
                }

                Nethereum.Web3.Accounts.Account mainAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(0).PrivateKey);

                int walletsCreated = 0;

                while (true)
                {
                    if (cancelPressed == true)
                    {
                        return;
                    }

                    var accountfromDB = _dataController.GetClaimByIdAndChain(claimId, _blockchainController.ChainName, _accountId);

                    if (accountfromDB == null)
                    {
                        Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(claimId).PrivateKey);
                        var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                        //Does not have a rank
                        if (HasClaimedRank(claimId, userMintResult) == false)
                        {
                            GasPrice gasPrice;
                            BigInteger claimRankTransactionFee = 0;

                            BigInteger claimRankGas = await _xenBlockchainController.EstimateGasToClaim(mintAccount.Address, termDays);
                            BigInteger transferGas = await _blockchainController.EstimateCoinTransferGas(mainAccount.Address, mintAccount.Address, claimRankGas);

                            var seedAccountBalance = await _blockchainController.Getbalance(mainAccount.Address);
                            var mintAccountBalance = await _blockchainController.Getbalance(mintAccount.Address);

                            while (true)
                            {
                                gasPrice = await _blockchainController.EstimateGasPriceAsync(_priorityFee);

                                claimRankTransactionFee = gasPrice.Priority * claimRankGas;

                                var maxFeeWillingToPay = Web3.Convert.ToWei((decimal.Parse(MaxGasCost.Text) / _price));

                                if (maxFeeWillingToPay > claimRankTransactionFee)
                                {
                                    break;
                                }

                                await Task.Delay(60000);

                                if (cancelPressed == true)
                                {
                                    return;
                                }
                            }

                            if (cancelPressed == true)
                            {
                                return;
                            }

                            var transferFee = gasPrice.Priority * transferGas;
                            var claimFee = gasPrice.Priority * claimRankGas;
                            var transferAmount = transferFee + claimFee;

                            if (seedAccountBalance <= transferAmount)
                            {
                                MessageBox.Show("Done - ran out of money");
                                break;
                            }

                            BigInteger amountToSend = mintAccountBalance >= claimRankTransactionFee ? new BigInteger(0) : claimRankTransactionFee - mintAccountBalance;
                            var transaction = await _blockchainController.TransferCoins(_wallet.GetAccount(0), _wallet.GetAccount(claimId).Address, amountToSend, _priorityFee, transferGas, gasPrice);
                            await _xenBlockchainController.WaitForConfirmations(transaction);
                            await _xenBlockchainController.ClaimRank(_wallet.GetAccount(claimId), termDays, claimRankGas, gasPrice);

                            walletsCreated++;

                            DateTime claimExpire = DateTime.UtcNow.AddDays(termDays);
                            string address = _wallet.GetAccount(claimId).Address;
                            UserMintOutputDTO data;
                            long tokens = 0;

                            while (true)
                            {
                                data = await _xenBlockchainController.GetUserMints(address);
                                tokens = _xenBlockchainController.GetGrossReward((long)data.Rank + 10, (long)data.Amplifier, (long)data.Term, (long)data.EaaRate, (long)data.Rank);
                                if (tokens > 0)
                                {
                                    break;
                                }
                                await Task.Delay(2000);
                            }

                            _dataController.UpdateClaimInDB(claimId, _accountId, claimExpire, address, chain, (long)data.Rank, (long)data.Amplifier, (long)data.EaaRate, (long)data.Term, tokens);
                            await Task.Delay(2000);
                        }
                        else
                        {
                            var term = userMintResult.Term;
                            var matureDate = UnixTimeStampToDateTime((long)userMintResult.MaturityTs);
                            var address = userMintResult.Address;
                            var tokens = _xenBlockchainController.GetGrossReward(_globalRank, (long)userMintResult.Amplifier, (long)userMintResult.Term, (long)userMintResult.EaaRate, (long)userMintResult.Rank);
                            _dataController.UpdateClaimInDB(claimId, _accountId, UnixTimeStampToDateTime((long)userMintResult.MaturityTs), address, chain, (long)userMintResult.Rank, (long)userMintResult.Amplifier, (long)userMintResult.EaaRate, (long)userMintResult.Term, tokens);
                        }
                        LoadTotals();

                        if (_wallets <= walletsCreated)
                        {
                            break;
                        }
                    }

                    claimId++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cancelPressed = false;
                CancelBtn.IsEnabled = false;
                EnableApp(true);
                LoadTokens();
                LoadClaimsDue();
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
            Claims.Remove(Claims.SingleOrDefault(i => i.AccountId == e.Account.Id));
        }

        private void UpdateAccountToGrid(Entities.Claim account)
        {
            ClaimVM dgObj = Claims.FirstOrDefault(x => x.AccountId == account.Id && x.Chain == account.Chain);

            if(dgObj != null)
            {
                dgObj.Address = account.Address;
                dgObj.AccountId = account.Id;
                dgObj.Chain = account.Chain;
                dgObj.ClaimExpire = account.ClaimExpire;
                dgObj.DaysLeft = account.DaysLeft;
                dgObj.EstimatedTokens = string.Format(CultureInfo.InvariantCulture, "{0:n0}", account.Tokens);
            }
            else
            {
                AddAccountToGrid(account);
            }
        }

        private void AddAccountToGrid(Entities.Claim account)
        {
            ClaimVM dgObj = new ClaimVM();
            dgObj.Address = account.Address;
            dgObj.AccountId = account.Id;
            dgObj.Chain = account.Chain;
            dgObj.ClaimExpire = account.ClaimExpire;
            dgObj.DaysLeft = account.DaysLeft;
            dgObj.EstimatedTokens = string.Format(CultureInfo.InvariantCulture, "{0:n0}", account.Tokens);
            Claims.Add(dgObj);
        }

        private void EnableApp(bool isEnabled)
        {
            UpdatePriceButton.IsEnabled = isEnabled;
            MaxGasCost.IsEnabled = isEnabled;
            tbWallets.IsEnabled = isEnabled;
            TermDays.IsEnabled = isEnabled;
            ClaimRankBtn.IsEnabled = isEnabled;
            cbBlockChain.IsEnabled = isEnabled;
            btnGetExistingAccounts.IsEnabled = isEnabled;
            cbPriorityFee.IsEnabled = isEnabled;
            ShowPrivateKey.IsEnabled = isEnabled;
            UpdatePriceButton.IsEnabled = isEnabled;
            btnRefreshData.IsEnabled = isEnabled;
            btnClaimRewards.IsEnabled = isEnabled;
            cbAccount.IsEnabled = isEnabled;
            btnAddAccount.IsEnabled = isEnabled;
            btnDeleteAccount.IsEnabled = isEnabled;
        }

        private bool HasClaimedRank(int accountId, UserMintOutputDTO userMintResult)
        {
            return userMintResult.Address == _wallet.GetAccount(accountId).Address;
        }

        private ClaimRewardResponseStatus GetClaimRewardStatus(int accountId, UserMintOutputDTO userMintResult)
        {
            ClaimRewardResponseStatus status = ClaimRewardResponseStatus.Matured;

            DateTime now = DateTime.Now;

            DateTime expireDate = DateUtils.ConvertUnixTimestampToDate((long)userMintResult.MaturityTs).ToLocalTime();
            
            if(userMintResult.Address != _wallet.GetAccount(accountId).Address)
            {
                status = ClaimRewardResponseStatus.NotFound;
            }
            else if(now <= expireDate)
            {
                status = ClaimRewardResponseStatus.NotMatured;
            }

            return status;
        }

        //private async Task ClaimRank(int accountId, Wallet wallet, BigInteger gas, int termDays)
        //{
        //    await _xenBlockchainController.ClaimRank(wallet.GetAccount(accountId), termDays, _priorityFee);
        //}

        //private async Task<bool> SendMoneyToMintAccount(int accountId, Wallet wallet, BigInteger amountToSend, GasPrice gasPrice, BigInteger gas)
        //{
        //    //No need to send money to main account
        //    if(accountId == 0)
        //    {
        //        return null;
        //    }

        //    return await _blockchainController.TransferCoins(wallet.GetAccount(0), _wallet.GetAccount(accountId).Address, amountToSend, _priorityFee, gas, gasPrice);
        //}

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
            if (IsLoaded && _blockChain != cbBlockChain.Text)
            {
                try
                {
                    _blockChain = cbBlockChain.Text;
                    EnableApp(false);
                    LoadFactories();
                    RefreshGrid();
                    await LoadInfo();
                    LoadTokens();
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
                CancelBtn.IsEnabled = true;
                EnableApp(false);

                int claimId = 0;

                string chain = _blockchainController.ChainName;

                Nethereum.Web3.Accounts.Account mainAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(0).PrivateKey);

                int notMintedCount = 0;

                while (true)
                {
                    var accountfromDB = _dataController.GetClaimByIdAndChain(claimId, chain, _accountId);

                    if (accountfromDB == null)
                    {
                        Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(claimId).PrivateKey);
                        var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                        //Does not have a rank
                        if (HasClaimedRank(claimId, userMintResult) == false)
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
                            _dataController.UpdateClaimInDB(claimId, _accountId, matureDate, address, chain, (long)userMintResult.Rank, (long)userMintResult.Amplifier, (long)userMintResult.EaaRate, (long)userMintResult.Term, tokens);
                            LoadTotals();
                        }
                    }
                    else
                    {
                        notMintedCount = 0;

                        if (accountfromDB.ClaimExpire != null && DateTime.UtcNow > accountfromDB.ClaimExpire) //is expired
                        {
                            //var xx = _wallet.GetAccount(accountId).PrivateKey;
                            Nethereum.Web3.Accounts.Account mintAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(claimId).PrivateKey);
                            var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                            if(userMintResult.Address != mintAccount.Address) // It's claimed. So remove in local database
                            {
                                _dataController.DeleteClaimByIdAndChain(claimId, chain, _accountId);
                            }
                        }
                    }

                    if (notMintedCount >= 50)
                    {
                        break;
                    }

                    claimId++;

                    if (cancelPressed == true)
                    {
                        return;
                    }
                }

                RefreshGrid();
                LoadTokens();
            }
            catch (Exception ex)
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

        private void LoadTokens()
        {
            long tokens = 0;

            var dict = _dataController.AggregateTokensByChain(_accountId);

            if (dict.ContainsKey(cbBlockChain.Text))
            {
                tokens = dict[cbBlockChain.Text];
            }

            string tokenStr = string.Format(CultureInfo.InvariantCulture, "{0:n0}", tokens);

            txtEstimatedTokens.Text = tokenStr;
        }

        private void tbWallets_TextChanged(object sender, TextChangedEventArgs e)
        {
            _wallets = 0;
            int dummy = 0;
            if(int.TryParse(tbWallets.Text, out dummy) == false)
            {
                _wallets = 0;
            }
            else
            {
                _wallets = dummy;
            }
        }

        private async void btnRefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadClaimsDue();
                EnableApp(false);
                await LoadInfo();
                LoadTokens();
                _globalRank = await _xenBlockchainController.GetGlobalRank();
            }
            finally
            {
                EnableApp(true);
            }
        }

        private async void btnClaimRewards_Click(object sender, RoutedEventArgs e)
        {
            Nethereum.Web3.Accounts.Account mainAccount = new Nethereum.Web3.Accounts.Account(_wallet.GetAccount(0).PrivateKey);

            ClaimRewardWindow crWindow = new ClaimRewardWindow(mainAccount.Address);

            string claimRewardAddress;

            if (crWindow.ShowDialog() == true)
            {
                try
                {
                    claimRewardAddress = crWindow.Address;

                    CancelBtn.IsEnabled = true;
                    EnableApp(false);

                    string chain = _blockchainController.ChainName;

                    decimal maxClaimRankGasInDollars = 0;

                    if (string.IsNullOrWhiteSpace(MaxGasCost.Text) || decimal.TryParse(MaxGasCost.Text, out maxClaimRankGasInDollars) == false)
                    {
                        MessageBox.Show("Invalid max gas cost");
                        return;
                    }

                    int retries = 0;

                    while (true)
                    {
                        try
                        {
                            if (cancelPressed == true)
                            {
                                return;
                            }

                            var expiredAccounts = _dataController.GetExpiredClaimsByChain(chain, _accountId);

                            foreach (var expiredAccount in expiredAccounts)
                            {
                                var mintAccount = _wallet.GetAccount(expiredAccount.Id);

                                var userMintResult = await _xenBlockchainController.GetUserMints(mintAccount.Address);

                                ClaimRewardResponseStatus status = GetClaimRewardStatus(expiredAccount.Id, userMintResult);

                                if (status == ClaimRewardResponseStatus.Matured)
                                {
                                    GasPrice gasPrice;
                                    BigInteger mintRewardTransactionFee = 0;

                                    BigInteger claimRewardGas = await _xenBlockchainController.EstimateGasToMintReward(mintAccount.Address, mainAccount.Address);
                                    BigInteger transferGas = await _blockchainController.EstimateCoinTransferGas(mainAccount.Address, mintAccount.Address, claimRewardGas);

                                    var seedAccountBalance = await _blockchainController.Getbalance(mainAccount.Address);
                                    var mintAccountBalance = await _blockchainController.Getbalance(mintAccount.Address);

                                    while (true)
                                    {
                                        gasPrice = await _blockchainController.EstimateGasPriceAsync(_priorityFee);

                                        mintRewardTransactionFee = gasPrice.Priority * claimRewardGas;

                                        var maxFeeWillingToPay = Web3.Convert.ToWei((decimal.Parse(MaxGasCost.Text) / _price));

                                        if (maxFeeWillingToPay > mintRewardTransactionFee)
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

                                    var transferFee = gasPrice.Priority * transferGas;
                                    var claimRewardFee = gasPrice.Priority * claimRewardGas;
                                    var transferAmount = transferFee + claimRewardFee;

                                    if (seedAccountBalance <= transferAmount)
                                    {
                                        MessageBox.Show("Done - ran out of money");
                                        break;
                                    }

                                    BigInteger amountToSend = mintAccountBalance >= mintRewardTransactionFee ? new BigInteger(0) : mintRewardTransactionFee - mintAccountBalance;

                                    //await SendMoneyToMintAccount(expiredAccount.AccountId, _wallet, amountToSend, gasPrice, transferGas);
                                    await _blockchainController.TransferCoins(_wallet.GetAccount(0), expiredAccount.Address, amountToSend, _priorityFee, transferGas, gasPrice);
                                    await _xenBlockchainController.MintRewardAndShare(mintAccount, claimRewardAddress, claimRewardGas, gasPrice);

                                    _dataController.DeleteClaimByIdAndChain(expiredAccount.Id, chain, _accountId);
                                }
                                else if(status == ClaimRewardResponseStatus.NotMatured)
                                {
                                    var term = userMintResult.Term;
                                    var matureDate = UnixTimeStampToDateTime((long)userMintResult.MaturityTs);
                                    var address = userMintResult.Address;
                                    var tokens = _xenBlockchainController.GetGrossReward(_globalRank, (long)userMintResult.Amplifier, (long)userMintResult.Term, (long)userMintResult.EaaRate, (long)userMintResult.Rank);
                                    _dataController.UpdateClaimInDB(expiredAccount.Id, _accountId, UnixTimeStampToDateTime((long)userMintResult.MaturityTs), address, chain, (long)userMintResult.Rank, (long)userMintResult.Amplifier, (long)userMintResult.EaaRate, (long)userMintResult.Term, tokens);
                                }
                                else if(status == ClaimRewardResponseStatus.NotFound)
                                {
                                    _dataController.DeleteClaimByIdAndChain(expiredAccount.Id, chain, _accountId);
                                }
                                else
                                {
                                    throw new Exception("Could not determine status of mint");
                                }

                                LoadTotals();
                            }

                            break;
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("funds"))
                            {
                                throw;
                            }
                            else if (retries >= 5)
                            {
                                throw;
                            }
                            else
                            {
                                await Task.Delay(10000);
                                retries++;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    cancelPressed = false;
                    CancelBtn.IsEnabled = false;
                    EnableApp(true);
                    LoadTokens();
                    LoadClaimsDue();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string chain = _blockchainController.ChainName;

                var claims = _dataController.GetClaimsByChain(chain, _accountId);

                foreach (var claim in claims)
                {
                    var grossReward = _xenBlockchainController.GetGrossReward(_globalRank, claim.Amplifier, claim.Term, claim.EaaRate, claim.Rank);
                    _dataController.UpdateTokensByIdAndChain(claim.Id, _accountId, chain, grossReward);
                }

                RefreshGrid();
                LoadTokens();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateAccountWindow createAccountWindow = new CreateAccountWindow(_password);
            createAccountWindow.ShowDialog();

            LoadAccounts();
            var lastId = Accounts.Last().Id;

            try
            {
                _accountId = lastId;
                cbAccount.SelectedValue = lastId;
                EnableApp(false);
                LoadWallet();
                LoadFactories();
                RefreshGrid();
                await LoadInfo();
                LoadTokens();
                LoadTotals();
            }
            finally
            {
                EnableApp(true);
            }
        }

        private async void cbAccount_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded && _accountId != (int)cbAccount.SelectedValue)
            {
                try
                {
                    _accountId = (int)cbAccount.SelectedValue;
                    LoadWallet();
                    EnableApp(false);
                    LoadFactories();
                    RefreshGrid();
                    await LoadInfo();
                    LoadTokens();
                    LoadTotals();
                }
                finally
                {
                    EnableApp(true);
                }
            }
        }

        private void ChangePassowrdClick(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow();

            if(changePasswordWindow.ShowDialog() == true)
            {
                try
                {
                    var accounts = _dataController.GetAllAccountsWithPhrase();

                    for (int i = 0; i < accounts.Count; i++)
                    {
                        string oldKey = Rijndael.Decrypt(accounts[i].Phrase, _password, KeySize.Aes256);
                        string newEncryptedKey = Rijndael.Encrypt(oldKey, changePasswordWindow.NewPassword, KeySize.Aes256);
                        accounts[i].Phrase = newEncryptedKey;
                    }

                    _dataController.ChangePassword(accounts);

                    _password = changePasswordWindow.NewPassword;

                    LoadWallet();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Something went wrong");
                    return;
                }
            }
        }
    }
}
