using Messenger.Constants;
using Messenger.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class UserRepositoryInputValidatorTests
    {
        private readonly string connectionString = MiscConstants.ConnectionString;

        private readonly List<Guid> tempUsers = new List<Guid>();

        [TestMethod]
        public void CheckEmailUniqueness_UniqueEmail_True()
        {
            // arrange
            var user = new User
            {
                Email = "testEmail@mail.ru",
            };

            // act
            var userRepository = new UserRepository(connectionString);
            var validator = new UserRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository);
            var privateValidator = new PrivateObject(validator);

            bool unique = false;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    unique = (bool)privateValidator.Invoke("CheckEmailUniqueness", new object[] { user });
                }
            }

            // asserts
            Assert.IsTrue(unique);
        }

        [TestMethod]
        public void CheckEmailUniqueness_NotUniqueEmail_False()
        {
            // arrange
            var createdUser = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
            };

            var userWithNotUniqueEmail = new User
            {
                Name = "testName",
                LastName = "testLastName",
                Email = "testEmail@mail.ru",
                Password = "testPassword"
            };

            // act
            var userRepository = new UserRepository(connectionString);
            createdUser = userRepository.Create(createdUser);

            var validator = new UserRepositoryInputValidator(connectionString, new AttachmentRepository(connectionString), userRepository);
            var privateValidator = new PrivateObject(validator);

            bool unique = true;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    unique = (bool)privateValidator.Invoke("CheckEmailUniqueness", new object[] { userWithNotUniqueEmail });
                }
            }

            tempUsers.Add(createdUser.Id);

            // asserts
            Assert.IsFalse(unique);
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
