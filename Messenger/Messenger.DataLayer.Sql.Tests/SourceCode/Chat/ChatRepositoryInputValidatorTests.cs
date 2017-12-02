using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.Generic;
using Messenger.Constants;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class ChatRepositoryInputValidatorTests
    {
        private readonly string connectionString = MiscConstants.ConnectionString;

        private readonly List<Guid> tempChats = new List<Guid>();
        private List<User> tempChatMembers = new List<User>();
        private List<User> tempChatMembers2 = new List<User>();

        [TestMethod]
        public void OnePersonChatExists_Exists_True()
        {
            // arrange
            var testUser1 = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };
            var chatMembers = new List<User>() { testUser1 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);

            var testChat = new Chat()
            {
                Members = chatMembers.AsReadOnly()
            };
            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat = chatRepository.Create(testChat);

            // act
            var validator = new ChatRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository, chatRepository);
            var privateValidator = new PrivateObject(validator);
            bool actualResult;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    actualResult = (bool)privateValidator.Invoke("OnePersonChatExists", new object[] { command, testChat });
                }
            }

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.IsTrue(actualResult);
        }

        [TestMethod]
        public void OnePersonChatExists_DoesntExist_False()
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
            var chat1Members = new List<User>() { testUser1 };
            var chat2Members = new List<User>() { testUser2 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);

            var testChat1 = new Chat()
            {
                Members = chat1Members.AsReadOnly()
            };
            var testChat2 = new Chat()
            {
                Members = chat2Members.AsReadOnly()
            };
            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat1 = chatRepository.Create(testChat1);

            // act
            var validator = new ChatRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository, chatRepository);
            var privateValidator = new PrivateObject(validator);
            bool actualResult;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    actualResult = (bool)privateValidator.Invoke("OnePersonChatExists", new object[] { command, testChat2 });
                }
            }

            tempChats.Add(testChat1.Id);
            tempChats.Add(testChat2.Id);
            tempChatMembers = chat1Members;
            tempChatMembers2 = chat2Members;

            // asserts
            Assert.IsFalse(actualResult);
        }

        [TestMethod]
        public void TwoPersonsChatExists_Exists_True()
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

            // act
            var validator = new ChatRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository, chatRepository);
            var privateValidator = new PrivateObject(validator);
            bool actualResult;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    actualResult = (bool)privateValidator.Invoke("TwoPersonsChatExists", new object[] { command, testChat });
                }
            }

            tempChats.Add(testChat.Id);
            tempChatMembers = chatMembers;

            // asserts
            Assert.IsTrue(actualResult);
        }

        [TestMethod]
        public void TwoPersonsChatExists_DoesntExist_False()
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
            var chat1Members = new List<User>() { testUser1, testUser2 };
            var chat2Members = new List<User>() { testUser2, testUser3 };
            var userRepository = new UserRepository(connectionString);

            testUser1 = userRepository.Create(testUser1);
            testUser2 = userRepository.Create(testUser2);
            testUser3 = userRepository.Create(testUser3);

            var testChat1 = new Chat()
            {
                Members = chat1Members.AsReadOnly()
            };
            var testChat2 = new Chat()
            {
                Members = chat2Members.AsReadOnly()
            };
            var chatRepository = new ChatRepository(connectionString, userRepository);
            testChat1 = chatRepository.Create(testChat1);

            // act
            var validator = new ChatRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository, chatRepository);
            var privateValidator = new PrivateObject(validator);
            bool actualResult;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    actualResult = (bool)privateValidator.Invoke("TwoPersonsChatExists", new object[] { command, testChat2 });
                }
            }

            tempChats.Add(testChat1.Id);
            tempChats.Add(testChat2.Id);
            tempChatMembers = chat1Members;
            tempChatMembers2.Add(chat2Members[1]);

            // asserts
            Assert.IsFalse(actualResult);
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

            for (int i = 0; i < tempChatMembers2.Count; i++)
                userRepository.Delete(tempChatMembers2[i].Id);
        }
    }
}
