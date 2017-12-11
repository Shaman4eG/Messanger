using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using System.Collections.Generic;
using Messenger.Constants;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class ChatRepositoryTests
    {
        private readonly string connectionString = MiscConstants.ConnectionString;

        private readonly List<Guid> tempChats = new List<Guid>();
        private List<User> tempChatMembers = new List<User>();



        [TestMethod]
        public void Create_CreatePersonalChat_SameChatReturned()
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

            var expectedChat = new Chat()
            {
                Type = ChatType.personal,
                Members = chatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            var actualChat = chatRepository.Create(expectedChat);

            tempChats.Add(expectedChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.AreEqual(expectedChat.Id, actualChat.Id);
            Assert.AreEqual(expectedChat.Type, actualChat.Type);

            for (int i = 0; i < expectedChat.Members.Count; i++)
                Assert.AreEqual(expectedChat.Members[i].Id, actualChat.Members[i].Id);
        }

        [TestMethod]
        public void Create_CreateGroupChat_SameChatReturned()
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
            var testUser3 = new User
            {
                Name = "testName3",
                LastName = "testLastName3",
                Email = "testEmail3@mail.ru",
                Password = "testPassword3",
            };
            var chatMembers = new List<User>() { testUser1, testUser2, testUser3 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);
            testUser3 = userRepository.Create(testUser3);

            var expectedChat = new Chat()
            {
                Type = ChatType.group,
                AdminId = testUser1.Id,
                Name = "Test Group Chat",
                Members = chatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            var actualChat = chatRepository.Create(expectedChat);

            tempChats.Add(expectedChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.AreEqual(expectedChat.Id, actualChat.Id);
            Assert.AreEqual(expectedChat.Type, actualChat.Type);
            Assert.AreEqual(expectedChat.Name, actualChat.Name);

            for (int i = 0; i < expectedChat.Members.Count; i++)
                Assert.AreEqual(expectedChat.Members[i].Id, actualChat.Members[i].Id);
        }

        [TestMethod]
        public void Create_SaveChatToDb_SameChatInDb()
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

            var expectedChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            chatRepository.Create(expectedChat);
            var chatFromDb = chatRepository.Get(expectedChat.Id);

            tempChats.Add(expectedChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.AreEqual(expectedChat.Id, chatFromDb.Id);
        }

        [TestMethod]
        public void Create_MembersNotSet_Null()
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

            var expectedChat = new Chat();

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            var createdChat = chatRepository.Create(expectedChat);

            tempChats.Add(expectedChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.IsNull(createdChat);
        }

        [TestMethod]
        public void Get_ChatFound_RequestedChat()
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

            var expectedChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            chatRepository.Create(expectedChat);
            var chatFromDb = chatRepository.Get(expectedChat.Id);

            tempChats.Add(expectedChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.AreEqual(expectedChat.Id, chatFromDb.Id);
        }

        [TestMethod]
        public void Get_ChatNotFound_Null()
        {
            // arrange
            var randomId = Guid.NewGuid();
            var userRepository = new UserRepository(connectionString);

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            var chatFromDb = chatRepository.Get(randomId);

            // asserts
            Assert.IsNull(chatFromDb);
        }

        [TestMethod]
        public void GetUserChats_ChatsFound_SameTwoChatsInDb()
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
            var testUser3 = new User
            {
                Name = "testName3",
                LastName = "testLastName3",
                Email = "testEmail3@mail.ru",
                Password = "testPassword3",
            };
            var firstChatMembers = new List<User>() { testUser1, testUser2 };
            var secondChatMembers = new List<User>() { testUser1, testUser3 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);
            testUser3 = userRepository.Create(testUser3);

            var expectedFirstChat = new Chat()
            {
                AdminId = testUser1.Id,
                Name = "Test Users Chat 1",
                Members = firstChatMembers.AsReadOnly()
            };
            var expectedSecondChat = new Chat()
            {
                AdminId = testUser1.Id,
                Name = "Test Users Chat 2",
                Members = secondChatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            expectedFirstChat = chatRepository.Create(expectedFirstChat);
            expectedSecondChat = chatRepository.Create(expectedSecondChat);
            var expectedTestUser1Chats = new List<Chat> { expectedFirstChat, expectedSecondChat };
            var actualTestUser1Chats = chatRepository.GetUserChats(testUser1.Id);

            tempChats.Add(expectedFirstChat.Id);
            tempChats.Add(expectedSecondChat.Id);
            tempChatMembers.Add(testUser1);
            tempChatMembers.Add(testUser2);
            tempChatMembers.Add(testUser3);

            // TODO: override Equals() + CollectionAssert.AreEquivalent ???
            // asserts
            bool found;
            // Checks that chat Ids are the same.
            for (int j = 0; j < expectedTestUser1Chats.Count; j++)
            {
                found = false;
                for (int i = 0; i < actualTestUser1Chats.Count; i++)
                {
                    if (expectedTestUser1Chats[j].Id == actualTestUser1Chats[i].Id)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found);
            }
        }

        [TestMethod]
        public void Delete_RemoveChatFromDb_NoChatWithPassedIdInDb()
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

            var chatToDelete = new Chat()
            {
                Type = ChatType.personal,
                Members = chatMembers.AsReadOnly()
            };

            // act
            var chatRepository = new ChatRepository(connectionString, userRepository);
            chatRepository.Create(chatToDelete);
            var deleted = chatRepository.Delete(chatToDelete.Id);
            var foundChat = chatRepository.Get(chatToDelete.Id);

            tempChats.Add(chatToDelete.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.IsTrue(deleted);
            Assert.IsNull(foundChat);
        }

        [TestMethod]
        public void Delete_ChatNotFound_FalseReturned()
        {
            // arrange
            var randomId = Guid.NewGuid();

            // act
            var chatRepository = new ChatRepository(connectionString, new UserRepository(connectionString));
            var found = chatRepository.Delete(randomId);

            // asserts
            Assert.IsFalse(found);
        }



        [TestCleanup]
        public void Clean()
        {
            var userRepository = new UserRepository(connectionString);
            var chatRepository = new ChatRepository(connectionString, userRepository);

            foreach (var chatId in tempChats)
                chatRepository.Delete(chatId);

            for (int i = 0; i < tempChatMembers.Count; i++)
                userRepository.Delete(tempChatMembers[i].Id);
        }
    }
}
