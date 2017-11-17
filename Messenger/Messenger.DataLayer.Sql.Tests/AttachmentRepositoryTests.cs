using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using Messenger.Model;
using System.IO;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class AttachmentRepositoryTests
    {
        private readonly string connectionString = InputConstraintsAndDefaultValues.ConnectionString;
        private readonly List<Guid> tempAttachments = new List<Guid>();

        [TestMethod]
        public void Create_CreateAttachment_SameAttachmentReturned()
        {
            // arrange
            var expectedAttachment = new Attachment
            {
                Type = "png",
                File = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new AttachmentRepository(connectionString);
            var actualAttachmet = repository.Create(expectedAttachment);

            tempAttachments.Add(expectedAttachment.Id);

            // asserts
            Assert.AreEqual(expectedAttachment.Type, actualAttachmet.Type);
            CollectionAssert.AreEqual(expectedAttachment.File, actualAttachmet.File);
        }

        [TestMethod]
        public void Create_SaveAttachmentToDb_SameAttachmentInDb()
        {
            // arrange
            var expectedAttachment = new Attachment
            {
                Type = "png",
                File = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new AttachmentRepository(connectionString);
            expectedAttachment = repository.Create(expectedAttachment);
            var actualAttachmet = repository.Get(expectedAttachment.Id);

            tempAttachments.Add(expectedAttachment.Id);

            // asserts
            Assert.AreEqual(expectedAttachment.Type, actualAttachmet.Type);
            CollectionAssert.AreEqual(expectedAttachment.File, actualAttachmet.File);
        }

        [TestMethod]
        public void Delete_RemoveAttachmentFromDb_NoAttachmentWithPassedIdInDb()
        {
            // arrange
            var expectedAttachment = new Attachment
            {
                Type = "png",
                File = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new AttachmentRepository(connectionString);
            expectedAttachment = repository.Create(expectedAttachment);
            repository.Delete(expectedAttachment.Id);
            var actualAttachmet = repository.Get(expectedAttachment.Id);

            tempAttachments.Add(expectedAttachment.Id);

            // asserts
            Assert.IsNull(actualAttachmet);
        }

        [TestMethod]
        public void Get_AttachmentFound_RequestedAttachment()
        {
            // arrange
            var expectedAttachment = new Attachment
            {
                Type = "png",
                File = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new AttachmentRepository(connectionString);
            expectedAttachment = repository.Create(expectedAttachment);
            var actualAttachmet = repository.Get(expectedAttachment.Id);

            tempAttachments.Add(expectedAttachment.Id);

            // asserts
            Assert.AreEqual(expectedAttachment.Id, actualAttachmet.Id);
        }

        [TestMethod]
        public void Get_AttachmentNotFound_Null()
        {
            // arrange
            var randomId = Guid.NewGuid();

            // act
            var repository = new AttachmentRepository(connectionString);
            var recievedAttachment = repository.Get(randomId);

            // asserts
            Assert.IsNull(recievedAttachment);
        }

        [TestCleanup]
        public void Clean()
        {
            var repository = new AttachmentRepository(connectionString);

            foreach (var id in tempAttachments)
                repository.Delete(id);
        }
    }
}
