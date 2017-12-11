using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AddChatPage.xaml
    /// </summary>
    public partial class AddChatPage : Page
    {
        ObservableCollection<string> emailList;
        List<User> chatMembers;

        public AddChatPage()
        {
            emailList = new ObservableCollection<string>();
            emailList.Add(LogedUser.LogedInUser.Email);
            chatMembers = new List<User>();
            chatMembers.Add(LogedUser.LogedInUser);

            InitializeComponent();
            EmailList.ItemsSource = emailList;
        }

        public AddChatPage(ObservableCollection<string> emailList, List<User> chatMembers)
        {
            this.emailList = emailList;
            this.chatMembers = chatMembers;

            InitializeComponent();
            EmailList.ItemsSource = this.emailList;
        }


        private void AddEmail_Click(object sender, RoutedEventArgs e)
        {
            if (Email.Text == "")
            {
                MessageBox.Show("Email not given.", "Error", MessageBoxButton.OK);
                return;
            }

            if (Email.Text == LogedUser.LogedInUser.Email)
            {
                MessageBox.Show("You are already in chat.", "Error", MessageBoxButton.OK);
                return;
            }

            var userFound = CheckUserWithGivenEmailExistens(Email.Text);
            if (!userFound) return;

            AddEmailToList(Email.Text);

            Email.Text = "";
        }

        private bool CheckUserWithGivenEmailExistens(string email)
        {
            // '/' is needed to process query properly because of '.' in email.
            var response = MainWindow.client.GetAsync("users/" + Email.Text + "/").Result;
            if (response.IsSuccessStatusCode)
            {
                var newChatMember = response.Content.ReadAsAsync<User>().Result;
                // Check if user already added.
                foreach (var member in chatMembers)
                {
                    if (member.Id == newChatMember.Id)
                    {
                        return true;
                    }
                }
                chatMembers.Add(newChatMember);
                return true;
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + ":\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);

                return false;
            }
        }

        private void AddEmailToList(string email)
        {
            if (emailList.Contains(email)) return;

            emailList.Add(Email.Text);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChatsPage());
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            if (emailList.Count > 2)
            {
                NavigationService.Navigate(new CreateGroupChatPage(emailList, chatMembers));
                return;
            }

            // Personal
            var chat = new Chat { Members = chatMembers.AsReadOnly() };
            var response = MainWindow.client.PostAsJsonAsync("chats/create", chat).Result;
            if (response.IsSuccessStatusCode)
            {
                NavigationService.Navigate(new ChatsPage());
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + "\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);
            }
        }
    }
}
