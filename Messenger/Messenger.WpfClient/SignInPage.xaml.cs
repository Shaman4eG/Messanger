using Messenger.Constants;
using Messenger.DataLayer.Sql;
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
    /// Interaction logic for SignInPage.xaml
    /// </summary>
    public partial class SignInPage : Page
    {
        public SignInPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WelcomePage());
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (Email.Text == "")
            {
                MessageBox.Show("Email not given.", "Error", MessageBoxButton.OK);
                return;
            }

            // Last '/' is needed to process query properly because of '.' in email.
            var response = MainWindow.client.GetAsync("users/" + Email.Text + "/").Result;
            if (response.IsSuccessStatusCode)
            {
                var userToLogIn = response.Content.ReadAsAsync<User>().Result;
                var correctPassword = Authentication(userToLogIn);
                if (correctPassword)
                {
                    LogedUser.LogedInUser = userToLogIn;
                    NavigationService.Navigate(new ChatsPage());
                }
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + ":\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);
            }
        }

        private bool Authentication(User userToLogIn)
        {
            if (Password.Password == userToLogIn.Password) return true;
            else
            {
                MessageBox.Show("Wrong password.", "Error", MessageBoxButton.OK);
                return false;
            }
        }
    }
}
