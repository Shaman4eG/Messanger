using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Messenger.WpfClient
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        // TODO: Say what is invalid in input
        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var user = new User
            {
                Name = Name.Text,
                LastName = Lastname.Text,
                Email = Email.Text,
                Password = Password.Password
            };
            
            var response = MainWindow.client.PostAsJsonAsync("users/create", user).Result;
            if (response.IsSuccessStatusCode)
            {
                NavigationService.Navigate(new SignedUpSuccessfullyPage());
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + "\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error", 
                    MessageBoxButton.OK);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new WelcomePage());
        }
    }
}
