using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using System.Collections.Generic;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class ChatRepositoryTests
    {
        private readonly string connectionString = "Server=DANIEL;Database=Messenger;Trusted_Connection=true";

        private readonly List<Guid> tempChats = new List<Guid>();
        private List<Guid> tempUsersInChat = new List<Guid>();

        [TestMethod]
        public void Create_SavesChatToDb_SameChat()
        {
            // arrange
            var firstUserInChat = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };

            var secondUserInChat = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };

            var userRepository = new UserRepository(connectionString);
            firstUserInChat = userRepository.Create(firstUserInChat);
            secondUserInChat = userRepository.Create(secondUserInChat);

            var expectedChat = new Chat
            {
                Type = ChatType.personal,
                AdminId = firstUserInChat.Id,
            };

            List<Guid> usersIdsInChat = new List<Guid>() { firstUserInChat.Id, secondUserInChat.Id };

            var usersInChat = new List<User>();
            foreach (var userId in usersIdsInChat)
                usersInChat.Add(userRepository.Get(userId));
            expectedChat.Members = usersInChat.AsReadOnly();

            // act
            var repository = new ChatRepository(connectionString, new UserRepository(connectionString));
            var actualChat = repository.Create(usersIdsInChat.AsReadOnly(), expectedChat.AdminId);

            tempChats.Add(expectedChat.Id);
            tempUsersInChat = usersIdsInChat;

            // asserts
            Assert.AreEqual(expectedChat.Type, actualChat.Type);
            Assert.AreEqual(expectedChat.AdminId, actualChat.AdminId);
            // TODO: override Equals, use CollectionAssert
            for (int i = 0; i < expectedChat.Members.Count; i++)
                Assert.AreEqual(expectedChat.Members[i].Id, actualChat.Members[i].Id);
        }

        [TestMethod]
        public void Delete_DeleteChatFromDb_ChatDeleted()
        {
            // arrange
            var firstUserInChat = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
            };

            var secondUserInChat = new User
            {
                Name = "testName2",
                LastName = "testLastName2",
                Email = "testEmail2@mail.ru",
                Password = "testPassword2",
            };

            var userRepository = new UserRepository(connectionString);
            firstUserInChat = userRepository.Create(firstUserInChat);
            secondUserInChat = userRepository.Create(secondUserInChat);

            var chatToDelete = new Chat
            {
                Type = ChatType.personal,
                AdminId = firstUserInChat.Id,
            };

            List<Guid> usersIdsInChat = new List<Guid>() { firstUserInChat.Id, secondUserInChat.Id };

            var usersInChat = new List<User>();
            foreach (var userId in usersIdsInChat)
                usersInChat.Add(userRepository.Get(userId));
            chatToDelete.Members = usersInChat.AsReadOnly();

            // act
            var repository = new ChatRepository(connectionString, new UserRepository(connectionString));
            chatToDelete.Id = repository.Create(usersIdsInChat.AsReadOnly(), chatToDelete.AdminId).Id;
            repository.Delete(chatToDelete.Id);

            tempChats.Add(chatToDelete.Id);
            tempUsersInChat = usersIdsInChat;

            // asserts
            Assert.IsNull(repository.Get(chatToDelete.Id));
        }
    }
}
