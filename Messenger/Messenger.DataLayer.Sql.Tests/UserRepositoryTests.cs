using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using Messenger.Model;
using System.IO;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private readonly string connectionString = "Server=DANIEL;Database=Messenger;Trusted_Connection=true";

        private readonly List<Guid> tempUsers = new List<Guid>();

        [TestMethod]
        public void Create_CreateUser_SameUserReturned()
        {
            // arrange
            var expectedUser = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
                Avatar = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new UserRepository(connectionString);
            var actualUser = repository.Create(expectedUser);

            tempUsers.Add(expectedUser.Id);

            // asserts
            Assert.AreEqual(expectedUser.Name, actualUser.Name);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.Email, actualUser.Email);
            Assert.AreEqual(expectedUser.Password, actualUser.Password);
            Assert.AreEqual(expectedUser.Avatar, actualUser.Avatar);
        }

        [TestMethod]
        public void Create_SaveUserToDb_SameUserInDb()
        {
            // arrange
            var expectedUser = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
                Avatar = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new UserRepository(connectionString);
            repository.Create(expectedUser);
            var actualUser = repository.Get(expectedUser.Id);

            tempUsers.Add(expectedUser.Id);

            // asserts
            Assert.AreEqual(expectedUser.Name, actualUser.Name);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.Email, actualUser.Email);
            Assert.AreEqual(expectedUser.Password, actualUser.Password);
            CollectionAssert.AreEqual(expectedUser.Avatar, actualUser.Avatar);
        }

        [TestMethod]
        public void Delete_RemoveUserFromDb_NoUserWithPassedIdInDb()
        {
            // arrange
            var userToDelete = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
                Avatar = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new UserRepository(connectionString);
            userToDelete.Id = repository.Create(userToDelete).Id;
            repository.Delete(userToDelete.Id);
            var user = repository.Get(userToDelete.Id);

            tempUsers.Add(userToDelete.Id);

            // asserts
            Assert.IsNull(user);
        }

        [TestMethod]
        public void Get_UserFound_RequestedUser()
        {
            // arrange
            var userToFind = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword",
                Avatar = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new UserRepository(connectionString);
            userToFind.Id = repository.Create(userToFind).Id;
            var recievedUser = repository.Get(userToFind.Id);

            tempUsers.Add(userToFind.Id);

            // asserts
            Assert.AreEqual(userToFind.Id, recievedUser.Id);
        }

        [TestMethod]
        public void Get_UserNotFound_Null()
        {
            // arrange
            var randomId = Guid.NewGuid();

            // act
            var repository = new UserRepository(connectionString);
            var recievedUser = repository.Get(randomId);

            // asserts
            Assert.IsNull(recievedUser);
        }

        [TestCleanup]
        public void Clean()
        {
            var repository = new UserRepository(connectionString);

            foreach (var id in tempUsers)
                repository.Delete(id);
        }
    }
}
