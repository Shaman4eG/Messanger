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
    /// Interaction logic for CreateGroupChatPage.xaml
    /// </summary>
    public partial class CreateGroupChatPage : Page
    {
        ObservableCollection<string> emailList;
        List<User> chatMembers;

        public CreateGroupChatPage(ObservableCollection<string> emailList, List<User> chatMembers)
        {
            this.emailList = emailList;
            this.chatMembers = chatMembers;

            InitializeComponent();
            EmailList.ItemsSource = this.emailList;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddChatPage(emailList, chatMembers));
        }

        private void CreateGroupChat_Click(object sender, RoutedEventArgs e)
        {
            var adminId = GetAdminId();

            var chat = new Chat
            {
                Name = ChatName.Text,
                AdminId = adminId,
                Members = chatMembers.AsReadOnly()
            };

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

        private Guid GetAdminId()
        {
            var selectedItem = EmailList.SelectedItem;

            if (selectedItem == null) return Guid.Empty;

            // '/' is needed to process query properly because of '.' in email.
            var response = MainWindow.client.GetAsync("users/" + selectedItem.ToString() + "/").Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<User>().Result.Id;
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + ":\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);

                return Guid.Empty;
            }
        }
    }
}
