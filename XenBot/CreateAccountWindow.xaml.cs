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

namespace XenBot
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private string _password = string.Empty;
        private Wallet _wallet = null;

        public CreateAccountWindow(string password)
        {
            _password = password;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletPath = System.IO.Path.Combine(currentDirectory, "wallet.data");

            try
            {
                _wallet = new Wallet(Wordlist.English, WordCount.Twelve);
                string words = string.Join(" ", _wallet.Words);
                var encryptedWords = Rijndael.Encrypt(words, _password, KeySize.Aes256);
                File.WriteAllText(walletPath, encryptedWords);
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
            string walletPath = System.IO.Path.Combine(currentDirectory, "wallet.data");

            try
            {
                string seed = Seed.Password.ToString();
                _wallet = new Wallet(seed, null);
                var encryptedWords = Rijndael.Encrypt(seed, _password, KeySize.Aes256);
                File.WriteAllText(walletPath, encryptedWords);
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
