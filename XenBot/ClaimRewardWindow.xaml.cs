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

namespace XenBot
{
    /// <summary>
    /// Interaction logic for ClaimRewardWindow.xaml
    /// </summary>
    public partial class ClaimRewardWindow : Window
    {
        public string Address { get; set; }

        public ClaimRewardWindow(string address)
        {
            InitializeComponent();
            Address = address;
            Loaded += MainLoaded;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Address = tbAddress.Text;

            this.DialogResult = true;
            this.Close();
        }

        private void MainLoaded(object sender, RoutedEventArgs e)
        {
            tbAddress.Text = Address;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
