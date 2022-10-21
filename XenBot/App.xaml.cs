using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NBitcoin;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.HdWallet;

namespace XenBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string password = string.Empty;

            Wallet wallet = null;

            //Disable shutdown when the dialog closes
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            if(ShouldCreatePassword())
            {
                CreatePasswordWindow createPasswordWindow = new CreatePasswordWindow();

                if (createPasswordWindow.ShowDialog() == false)
                {
                    MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
                    Current.Shutdown(-1);
                    return;
                }
            }

            LoginWindow loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                password = loginWindow.PasswordText;
            }
            else
            {
                MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
                Current.Shutdown(-1);
                return;
            }

            string walletPath = System.IO.Path.Combine(currentDirectory, "wallet.data");

            if (File.Exists(walletPath) == false)
            {
                CreateAccountWindow createAccountWindow = new CreateAccountWindow(password);

                if (createAccountWindow.ShowDialog() == false)
                {
                    MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
                    Current.Shutdown(-1);
                    return;
                }
            }


            var mainWindow = new MainWindow(password);
            //Re-enable normal shutdown mode.
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Current.MainWindow = mainWindow; 
            mainWindow.Show();
        }

        private bool ShouldCreatePassword()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string credentialsPath = Path.Combine(currentDirectory, "cred.cred");
            return File.Exists(credentialsPath) == false;
        }
    }
}
