using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Messenger.Model;
using Messenger.Constants;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private readonly string connectionString = MiscConstants.ConnectionString;

        private readonly List<Guid> tempUsers = new List<Guid>();

        [TestMethod]
        public void Create_CreateUser_CreatedUserReturned()
        {
            // arrange
            var expectedUser = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
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
        }

        [TestMethod]
        public void Create_SaveUserToDb_CreatedUserFoundInDb()
        {
            // arrange
            var expectedUser = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
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
        }

        [TestMethod]
        public void Create_InvalidUserData_NullReturned()
        {
            // arrange
            var expectedUser = new User();

            // act
            var repository = new UserRepository(connectionString);
            var actualUser = repository.Create(expectedUser);

            tempUsers.Add(expectedUser.Id);

            // asserts
            Assert.IsNull(actualUser);
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
                Password = "testPassword"
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

        [TestMethod]
        public void Update_UpdateUser_UserInDbUpdated()
        {
            // arrange
            var userToUpdate = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
            };

            var newUserData = new User
            {
                Name = "updatedName",
                LastName = "updatedLastname",
                Email = "updatedEmail",
                Password = "updatedPassword"
            };

            // act
            var repository = new UserRepository(connectionString);
            newUserData.Id = repository.Create(userToUpdate).Id;
            var updated = repository.Update(newUserData);
            var userWithUpdatedData = repository.Get(newUserData.Id);

            tempUsers.Add(userToUpdate.Id); 

            // asserts
            Assert.IsTrue(updated ?? false);
            Assert.AreEqual(newUserData.Name, userWithUpdatedData.Name);
            Assert.AreEqual(newUserData.LastName, userWithUpdatedData.LastName);
            Assert.AreEqual(newUserData.Email, userWithUpdatedData.Email);
            Assert.AreEqual(newUserData.Password, userWithUpdatedData.Password);
        }

        [TestMethod]
        public void Update_InvalidData_UserDataUnchanged()
        {
            // arrange
            var userToUpdate = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
            };

            var newUserData = new User
            {
                Name = "",
                LastName = "updatedLastname",
                Email = "updatedEmail",
                Password = "updatedPassword"
            };

            // act
            var repository = new UserRepository(connectionString);
            newUserData.Id = repository.Create(userToUpdate).Id;
            var updated = repository.Update(newUserData);
            var userWithNotUpdatedData = repository.Get(newUserData.Id);

            tempUsers.Add(userToUpdate.Id);

            // asserts
            Assert.IsFalse(updated ?? true);
            Assert.AreEqual(userToUpdate.Name, userWithNotUpdatedData.Name);
            Assert.AreEqual(userToUpdate.LastName, userWithNotUpdatedData.LastName);
            Assert.AreEqual(userToUpdate.Email, userWithNotUpdatedData.Email);
            Assert.AreEqual(userToUpdate.Password, userWithNotUpdatedData.Password);
        }

        [TestMethod]
        public void Update_UserNotFound_NullReturned()
        {
            // arrange
            var userWithRandomId = new User { Id = Guid.NewGuid() };

            // act
            var repository = new UserRepository(connectionString);
            var updated = repository.Update(userWithRandomId);

            // asserts
            Assert.IsNull(updated);
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
                Password = "testPassword"
            };

            // act
            var repository = new UserRepository(connectionString);
            userToDelete.Id = repository.Create(userToDelete).Id;
            var deleted = repository.Delete(userToDelete.Id);
            var user = repository.Get(userToDelete.Id);

            tempUsers.Add(userToDelete.Id);

            // asserts
            Assert.IsTrue(deleted);
            Assert.IsNull(user);
        }

        [TestMethod]
        public void Delete_UserNotFound_FalseReturned()
        {
            // arrange
            var randomId = Guid.NewGuid();

            // act
            var repository = new UserRepository(connectionString);
            var found = repository.Delete(randomId);

            // asserts
            Assert.IsFalse(found);
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
