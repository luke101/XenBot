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
    /// Interaction logic for CreatePasswordWindow.xaml
    /// </summary>
    public partial class CreatePasswordWindow : Window
    {
        public CreatePasswordWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Password.Password.ToString()))
            {
                MessageBox.Show("Password can't be blank");
                return;
            }

            string password = Password.Password.ToString();
            string confirm = Confirm.Password.ToString();

            if(password != confirm)
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            StorePassword(password);

            this.DialogResult = true;
            this.Close();
        }

        private void StorePassword(string password)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string credPath = System.IO.Path.Combine(currentDirectory, "cred.cred");

            byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            string hash = System.Text.Encoding.ASCII.GetString(data);

            System.IO.File.WriteAllText(credPath, hash);
        }
    }
}
