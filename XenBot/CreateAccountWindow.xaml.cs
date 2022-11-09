using NBitcoin;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.HdWallet;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Rijndael256;
using System.IO;
using Newtonsoft.Json;
using XenBot.DataControllers;
using XenBot.DatagridEntities;

namespace XenBot
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private string _password = string.Empty;
        private Wallet _wallet = null;
        private DataController _dataController;

        public CreateAccountWindow(string password)
        {
            _password = password;
            InitializeComponent();
            Loaded += WindowLoaded;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _dataController = new DataController();
            _dataController.CreateDFile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AccountNameWindow accountNameWindow = new AccountNameWindow();
            accountNameWindow.ShowDialog();

            try
            {
                _wallet = new Wallet(Wordlist.English, WordCount.Twelve);
                string words = string.Join(" ", _wallet.Words);
                var encryptedWords = Rijndael.Encrypt(words, _password, KeySize.Aes256);

                Entities.Account account = new Entities.Account();
                account.Name = accountNameWindow.Name;
                account.Address = _wallet.GetAccount(0).Address;
                account.Phrase = encryptedWords;
                _dataController.AddAccount(account);

                SeedPhraseWindow seedWindow = new SeedPhraseWindow(words);
                seedWindow.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AccountNameWindow accountNameWindow = new AccountNameWindow();
            accountNameWindow.ShowDialog();

            try
            {
                string seed = Seed.Text;
                _wallet = new Wallet(seed, null);
                var encryptedWords = Rijndael.Encrypt(seed, _password, KeySize.Aes256);

                Entities.Account account = new Entities.Account();
                account.Name = accountNameWindow.Name;
                account.Address = _wallet.GetAccount(0).Address;
                account.Phrase = encryptedWords;

                var accounts = _dataController.GetAllAccounts();
                if(accounts.Any(x => x.Address == _wallet.GetAccount(0).Address))
                {
                    throw new Exception("This account already added: " + accounts.First(x => x.Address == _wallet.GetAccount(0).Address).Name);
                }

                _dataController.AddAccount(account);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
