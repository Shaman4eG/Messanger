using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using System.Collections.Generic;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class MessageRepositoryTests
    {
        private readonly string connectionString = "Server=DANIEL;Database=Messenger;Trusted_Connection=true";

        private List<Guid> tempChats = new List<Guid>();
        private List<User> tempChatMembers = new List<User>();
        private List<Guid> tempMessages = new List<Guid>();

        [TestMethod]
        public void Send_MessageSent_SameMessageReturned()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var testUser2 = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };
            var chatMembers = new List<User>() { testUser1, testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            var expectedMessage = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text",
                SelfDeletion = false
            };

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            var actualMessage = messageRepository.Send(expectedMessage);

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;
            tempMessages.Add(expectedMessage.Id);

            // asserts
            Assert.AreEqual(expectedMessage.ChatId, actualMessage.ChatId);
            Assert.AreEqual(expectedMessage.AuthorId, actualMessage.AuthorId);
            Assert.AreEqual(expectedMessage.Date, actualMessage.Date);
            Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
            Assert.AreEqual(expectedMessage.AttachmentId, actualMessage.AttachmentId);
            Assert.AreEqual(expectedMessage.SelfDeletion, actualMessage.SelfDeletion);
        }

        [TestMethod]
        public void Send_MessageSent_MessageSavedToDb()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var testUser2 = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };
            var chatMembers = new List<User>() { testUser1, testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            var expectedMessage = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text",
                SelfDeletion = false
            };

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            expectedMessage = messageRepository.Send(expectedMessage);
            var actualMessage = messageRepository.Get(expectedMessage.Id);

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;
            tempMessages.Add(expectedMessage.Id);

            // asserts
            Assert.AreEqual(expectedMessage.Id, actualMessage.Id);
            Assert.AreEqual(expectedMessage.ChatId, actualMessage.ChatId);
            Assert.AreEqual(expectedMessage.AuthorId, actualMessage.AuthorId);
            Assert.AreEqual(expectedMessage.Date.Date, actualMessage.Date.Date);
            Assert.AreEqual(expectedMessage.Date.ToShortTimeString(), actualMessage.Date.ToShortTimeString());
            Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
            Assert.AreEqual(expectedMessage.AttachmentId, actualMessage.AttachmentId);
            Assert.AreEqual(expectedMessage.SelfDeletion, actualMessage.SelfDeletion);
        }

        [TestMethod]
        public void Delete_RemoveMessageFromDb_NoMessageWithPassedIdInDb()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var testUser2 = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };
            var chatMembers = new List<User>() { testUser1, testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            var messageToDelete = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text",
                SelfDeletion = false
            };

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            messageRepository.Send(messageToDelete);
            messageRepository.Delete(messageToDelete.Id);
            var foundMessage = messageRepository.Get(messageToDelete.Id);

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.IsNull(foundMessage);
        }

        [TestMethod]
        public void Get_MessageFound_RequestedMessage()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var testUser2 = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };
            var chatMembers = new List<User>() { testUser1, testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            var expectedMessage = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text",
                SelfDeletion = false
            };

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            messageRepository.Send(expectedMessage);
            var actualMessage = messageRepository.Get(expectedMessage.Id);

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;
            tempMessages.Add(expectedMessage.Id);

            // asserts
            Assert.AreEqual(expectedMessage.Id, actualMessage.Id);
        }

        [TestMethod]
        public void Get_MessageNotFound_Null()
        {
            // arrange
            var randomId = Guid.NewGuid();
            var userRepository = new UserRepository(connectionString);

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            var actualMessage = messageRepository.Get(randomId);

            // asserts
            Assert.IsNull(actualMessage);
        }

        [TestMethod]
        public void GetChatMessages_MessagesFound_SameMessagesRetrievedFromDb()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var testUser2 = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };
            var chatMembers = new List<User>() { testUser1, testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat = new Chat()
            {
                AdminId = testUser1.Id,
                Name = "Test Users Chat 1",
                Avatar = new byte[] { 1, 2, 3 },
                Members = chatMembers.AsReadOnly()
            };

            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            var testMessage1 = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text",
                SelfDeletion = false
            };
            var testMessage2 = new Message
            {
                ChatId = testChat.Id,
                AuthorId = testUser1.Id,
                Date = DateTime.Now,
                Text = "Test text2",
                SelfDeletion = false
            };

            // act
            var messageRepository = new MessageRepository(connectionString, userRepository);
            messageRepository.Send(testMessage1);
            messageRepository.Send(testMessage2);
            var expectedTestChatMessages = new List<Message> { testMessage1, testMessage2 };
            var actualTestChatMessages = messageRepository.GetChatMessages(testChat.Id);

            tempChats.Add(testChat.Id);
            tempChatMembers.Add(testUser1);
            tempChatMembers.Add(testUser2);
            tempMessages.Add(testMessage1.Id);
            tempMessages.Add(testMessage2.Id);

            // TODO: override Equals() + CollectionAssert.AreEquivalent ???
            // asserts
            bool found;
            // Checks that message Ids are the same.
            for (int j = 0; j < expectedTestChatMessages.Count; j++)
            {
                found = false;
                for (int i = 0; i < actualTestChatMessages.Count; i++)
                {
                    if (expectedTestChatMessages[j].Id == actualTestChatMessages[i].Id)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found);
            }
        }

        [TestCleanup]
        public void Clean()
        {
            var userRepository = new UserRepository(connectionString);
            var chatRepository = new ChatRepository(connectionString, userRepository);
            var messageRepository = new MessageRepository(connectionString, userRepository);

            foreach (var chatId in tempChats)
                chatRepository.Delete(chatId);

            for (int i = 0; i < tempChatMembers.Count; i++)
                userRepository.Delete(tempChatMembers[i].Id);

            foreach (var messageId in tempMessages)
                messageRepository.Delete(messageId);
        }
    }
}
