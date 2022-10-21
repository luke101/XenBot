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
using System.Windows.Shapes;

namespace XenBot
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string PasswordText { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string credPath = System.IO.Path.Combine(currentDirectory, "cred.cred");

            string storedHash = File.ReadAllText(credPath);

            byte[] data = System.Text.Encoding.ASCII.GetBytes(Password.Password.ToString());
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            string hash = System.Text.Encoding.ASCII.GetString(data);

            if (hash != storedHash)
            {
                MessageBox.Show("Wrong password");
                return;
            }

            PasswordText = Password.Password.ToString();

            this.DialogResult = true;
            this.Close();
        }
    }
}
