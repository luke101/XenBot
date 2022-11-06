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
using XenBot.DataControllers;
using XenBot.DatagridEntities;

namespace XenBot
{
    /// <summary>
    /// Interaction logic for AccountNameWindow.xaml
    /// </summary>
    public partial class AccountNameWindow : Window
    {
        public string Name { get; set; }

        public AccountNameWindow()
        {
            InitializeComponent();
            Loaded += AccountNameWindowLoaded;
        }

        private async void AccountNameWindowLoaded(object sender, RoutedEventArgs e)
        {
            DataController controller = new DataController();
            var accounts = controller.GetAllAccounts();

            int counter = 1;
            foreach(var account in accounts)
            {
                if(account.Name.ToUpper() == ("Account" + counter).ToUpper())
                {
                    counter++;
                }
            }

            tbName.Text = "Account" + counter;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataController controller = new DataController();
            var accounts = controller.GetAllAccounts();

            foreach(var account in accounts)
            {
                if(account.Name.ToUpper().Trim() == tbName.Text.ToUpper().Trim())
                {
                    MessageBox.Show("That name is already used");
                    return;
                }
            }

            Name = tbName.Text;

            this.DialogResult = true;
            this.Close();
        }
    }
}
