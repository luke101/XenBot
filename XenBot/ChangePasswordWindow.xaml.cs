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
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public string NewPassword { get; set; }

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataControllers.DataController dataController = new DataControllers.DataController();

            if (string.IsNullOrWhiteSpace(tbNewPassword.Password.ToString()))
            {
                MessageBox.Show("New password can't be blank");
                return;
            }

            if (tbNewPassword.Password.ToString() != tbConfirm.Password.ToString())
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string credPath = System.IO.Path.Combine(currentDirectory, "cred.cred");

            string storedHash = File.ReadAllText(credPath);

            byte[] data = System.Text.Encoding.ASCII.GetBytes(tbPassword.Password.ToString());
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            string hash = System.Text.Encoding.ASCII.GetString(data);

            if (hash != storedHash)
            {
                MessageBox.Show("Wrong password");
                return;
            }

            //////Change password

            byte[] newData = System.Text.Encoding.ASCII.GetBytes(tbNewPassword.Password.ToString());
            newData = new System.Security.Cryptography.SHA256Managed().ComputeHash(newData);
            string newHash = System.Text.Encoding.ASCII.GetString(newData);

            System.IO.File.WriteAllText(credPath, newHash);

            NewPassword = tbNewPassword.Password.ToString();

            this.DialogResult = true;
            this.Close();
        }
    }
}
