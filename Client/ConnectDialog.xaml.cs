using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private NetworkManager networkManager = NetworkManager.Instance;
        private bool alreadyConnecting = false;

        public ConnectDialog()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!alreadyConnecting) alreadyConnecting = true;
            else return;
            
            bool isConnected = false;
            try
            {
                isConnected = await networkManager.ConnectTcpClient(ServerIpBox.Text, int.Parse(PortNrBox.Text));
            } catch (FormatException ex)
            {
                MessageBox.Show("portnumber needs to be filled");
                return;
            }
            if (isConnected)
            {
                DialogResult = true; // Close the dialog
            } else
            {
                MessageBox.Show("connection not found. please try again");
            }

            alreadyConnecting = false;
        }
    }
}
