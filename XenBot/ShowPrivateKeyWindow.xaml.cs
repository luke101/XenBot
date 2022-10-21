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
    /// Interaction logic for SHowPrivateKeyWindow.xaml
    /// </summary>
    public partial class ShowPrivateKeyWindow : Window
    {
        public ShowPrivateKeyWindow(string privateKey)
        {
            InitializeComponent();
            tbPrivateKey.Text = privateKey;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
