using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using System.Windows.Threading;

namespace Messenger.WpfClient
{
    // TODO: invalid by date message output.
    // TODO: prevent attach jumping to other chats.
    // TODO: fix messages doubling and loss, when simultaneously messages from different clients are sent.
    // TODO: restrict images size visualization.
    /// <summary>
    /// Interaction logic for ChatsPage.xaml
    /// </summary>
    public partial class ChatsPage : Page
    {
        static ObservableCollection<Chat> chats;
        ObservableCollection<Message> chatMessagesGetter;
        List< ObservableCollection<ClientChatMessage> > chatMessages = new List<ObservableCollection<ClientChatMessage> >();
        ClientChatMessage clientChatMessage = new ClientChatMessage();
        static Attachment currentAttachment;

        DispatcherTimer dispatcherTimer;

        public ChatsPage()
        {
            InitializeComponent();

            LoadUserChats();

            ChatsHeaders.ItemsSource = chats;
        }

        private void LoadUserChats()
        {
            var response = MainWindow.client.GetAsync("chats/getUserChats/" + LogedUser.LogedInUser.Id).Result;
            if (response.IsSuccessStatusCode)
            {
                chats = SetPersonalChatNameToChatCompanionName(response.Content.ReadAsAsync<ReadOnlyCollection<Chat>>().Result);

                foreach (var chat in chats)
                    chatMessages.Add(new ObservableCollection<ClientChatMessage>());
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + ":\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);
            }
        }

        private ObservableCollection<Chat> SetPersonalChatNameToChatCompanionName(ReadOnlyCollection<Chat> foundChats)
        {
            var tempChats = new ObservableCollection<Chat>();

            foreach (var chat in foundChats)
            {
                if (chat.Type == ChatType.personal)
                {
                    // If solo chat - then set self name
                    if (chat.Members.Count == 1)
                    {
                        chat.Name = chat.Members[0].Name + " " + chat.Members[0].LastName;
                    }

                    foreach (var member in chat.Members)
                    {
                        if (member.Id != LogedUser.LogedInUser.Id)
                        {
                            chat.Name = member.Name + " " + member.LastName;
                            break;
                        }
                    }
                }
                tempChats.Add(chat);
            }

            return tempChats;
        }



        private void InitializeDispatcherTimeForNewMessagesFetching()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_CheckForNewMessages);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_CheckForNewMessages(object sender, EventArgs e)
        {
            var response = MainWindow.client.GetAsync(
                "messages/getChatNewMessages/" +
                chats[ChatsHeaders.SelectedIndex].Id + "/" +
                chatMessages[ChatsHeaders.SelectedIndex].Count).Result;

            if (response.IsSuccessStatusCode)
            {
                chatMessagesGetter = new ObservableCollection<Message>(
                    response.Content.ReadAsAsync<ReadOnlyCollection<Message>>()
                    .Result
                    .OrderBy(x => x.Date));

                foreach (var newMessage in chatMessagesGetter)
                {
                    //var attach = GetAttachment(newMessage.AttachmentId);
                    chatMessages[ChatsHeaders.SelectedIndex].Add(clientChatMessage.ConvertMessageToClientMessage(
                        newMessage, ChatsHeaders.SelectedIndex));
                    //newMessage.AttachmentId != Guid.Empty ? ConvertByteArrayToBitmapImage(attach.File, attach.Type) : null));
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



        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProfilePage());
        }

        private void Chats_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChatsPage());
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SearchPage());
        }

        private void AddChat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddChatPage());
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WelcomePage());
        }



        private void AddAttach_Click(object sender, RoutedEventArgs e)
        {
            var selectedSuccessfully = SelectFiles();
            if (!selectedSuccessfully) return;

            SendAttachment();
        }

        private void SendAttachment()
        {
            var message = new Message
            {
                ChatId = chats[ChatsHeaders.SelectedIndex].Id,
                AuthorId = LogedUser.LogedInUser.Id,
                Text = MessageText.Text,
                AttachmentId = currentAttachment.Id,
                SelfDeletion = false
            };

            var response = MainWindow.client.PostAsJsonAsync("messages/send", message).Result;
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    response.ReasonPhrase + "\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);
                return;
            }

            message = response.Content.ReadAsAsync<Message>().Result;
            var image = ConvertByteArrayToBitmapImage(currentAttachment.File, currentAttachment.Type);
            //chatMessages.Add(clientChatMessage.ConvertMessageToClientMessage(message, image));

            MessageText.Text = "";
            ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
            MessageText.Focus();
        }

        private bool SelectFiles()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Attachment  | *";
                // dlg.Multiselect = true;

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    var fileNameAndExtention = dlg.SafeFileName.Split('.');
                    return SaveAttachmentInDb(dlg.FileName, fileNameAndExtention[1]);
                }
                else return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return false;
            }
        }

        private bool SaveAttachmentInDb(string fileName, string type)
        {
            // Read bytes from file
            byte[] file;
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    file = reader.ReadBytes((int)stream.Length);
                }
            }

            var attach = new Attachment
            {
                Type = type,
                File = file
            };

            var response = MainWindow.client.PostAsJsonAsync("attachments/create", attach).Result;
            if (response.IsSuccessStatusCode)
            {
                currentAttachment = response.Content.ReadAsAsync<Attachment>().Result;
                return true;
            }
            else
            {
                MessageBox.Show(
                    response.ReasonPhrase + "\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);

                return false;
            }
        }

        private BitmapImage ConvertByteArrayToBitmapImage(byte[] file, string type)
        {
            BitmapImage image = null;
            if (type == "png" || type == "jpg")
            {
                image = new BitmapImage();
                using (var mem = new MemoryStream(file))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
            }

            return image;
        }





        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageText.Text == "") return;

            var message = new Message
            {
                ChatId = chats[ChatsHeaders.SelectedIndex].Id,
                AuthorId = LogedUser.LogedInUser.Id,
                Text = MessageText.Text,
                SelfDeletion = false
            };

            var response = MainWindow.client.PostAsJsonAsync("messages/send", message).Result;
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    response.ReasonPhrase + "\n\n" + response.Content.ReadAsStringAsync().Result,
                    "Error",
                    MessageBoxButton.OK);
                return;
            }

            message = response.Content.ReadAsAsync<Message>().Result;
            chatMessages[ChatsHeaders.SelectedIndex].Add(
                clientChatMessage.ConvertMessageToClientMessage(
                    message, 
                    ChatsHeaders.SelectedIndex));

            MessageText.Text = "";
            ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
            MessageText.Focus();
        }

        // If enter - send
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }



        private void ShowChatOnSelection(object sender, RoutedEventArgs e)
        {
            AddChatLbl.Visibility = Visibility.Hidden;
            ChatBox.Visibility = Visibility.Visible;

            GetChatMessages(chats[ChatsHeaders.SelectedIndex].Id);

            // Put scroller in chat to bottom, so last message is seen.
            if (ChatBox.Items.Count != 0) ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);

            InitializeDispatcherTimeForNewMessagesFetching();

            ChatBox.ItemsSource = chatMessages[ChatsHeaders.SelectedIndex];
            EnableChatButtons();
            MessageText.Text = "";
            MessageText.Focus();
        }

        private void GetChatMessages(Guid chatId)
        {
            var response = MainWindow.client.GetAsync("messages/getChatMessages/" + chatId).Result;
            if (response.IsSuccessStatusCode)
            {
                chatMessagesGetter = new ObservableCollection<Message>(
                    response.Content.ReadAsAsync< ReadOnlyCollection<Message> >()
                    .Result
                    .OrderBy(x => x.Date));

                foreach (var message in chatMessagesGetter)
                {
                    // var attach = GetAttachment(message.AttachmentId);
                    chatMessages[ChatsHeaders.SelectedIndex].Add(clientChatMessage.ConvertMessageToClientMessage(message, ChatsHeaders.SelectedIndex));//,
                        //message.AttachmentId != Guid.Empty ?  ConvertByteArrayToBitmapImage(attach.File, attach.Type) : null));
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

        private Attachment GetAttachment(Guid attachId)
        {
            var response = MainWindow.client.GetAsync("attachments/" + attachId).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<Attachment>().Result;
            }
            else
            {
                return null;
            }
        }

        private void EnableChatButtons()
        {
            SelfDelitable.IsEnabled = true;
            SendMessage.IsEnabled = true;
            AddAttach.IsEnabled = true;
            MessageText.IsEnabled = true;
        }



        public class ClientChatMessage
        {
            public Guid Id { get; set; }
            public string Date { get; set; }
            public string Name { get; set; }
            public string Lastname { get; set; }
            public string Text { get; set; }
            public BitmapImage Image { get; set; }

            internal ClientChatMessage ConvertMessageToClientMessage(Message message, int selectedChatIndex, BitmapImage img = null)
            {
                return new ClientChatMessage
                {
                    Id = message.Id,
                    Date = message.Date.ToString("G", DateTimeFormatInfo.InvariantInfo),
                    Name = GetMessageAuthor(message.AuthorId, selectedChatIndex)?.Name,
                    Lastname = GetMessageAuthor(message.AuthorId, selectedChatIndex)?.LastName,
                    Text = message.Text,
                    Image = img
                };
            }

            private static User GetMessageAuthor(Guid authorId, int selectedIndex)
            {
                return chats[selectedIndex].Members.Where(x => x.Id == authorId).ToList()[0];
            }
        }
    }




}
