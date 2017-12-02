using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Messenger.Model;
using System.Web.Http;
using Messenger.Constants;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class AttachmentRepositoryTests
    {
        private readonly string connectionString = MiscConstants.ConnectionString;
        private readonly List<Guid> tempAttachments = new List<Guid>();

        [TestMethod]
        public void Create_CreateAttachment_CreatedAttachmentReturned()
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
        public void Create_SaveAttachmentToDb_CreatedAttachmentInDb()
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
            CollectionAssert.AreEqual(expectedAttachment.File, actualAttachmet.File);
        }

        [TestMethod]
        public void Create_InvalidAttachmentData_NullReturned()
        {
            // arrange
            var expectedAttachment = new Attachment();

            // act
            var repository = new AttachmentRepository(connectionString);
            expectedAttachment = repository.Create(expectedAttachment);

            // asserts
            Assert.IsNull(expectedAttachment);
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

        [TestMethod]
        public void Delete_RemoveAttachmentFromDb_NoAttachmentWithPassedIdInDb()
        {
            // arrange
            var attachmentToDelete = new Attachment
            {
                Type = "png",
                File = new byte[] { 1, 2, 3 }
            };

            // act
            var repository = new AttachmentRepository(connectionString);
            attachmentToDelete = repository.Create(attachmentToDelete);
            var deleted = repository.Delete(attachmentToDelete.Id);
            var attachment = repository.Get(attachmentToDelete.Id);


            // asserts
            Assert.IsTrue(deleted);
            Assert.IsNull(attachment);
        }

        [TestMethod]
        public void Delete_UserNotFound_FalseReturned()
        {
            // arrange
            var randomId = Guid.NewGuid();

            // act
            var repository = new AttachmentRepository(connectionString);
            var found = repository.Delete(randomId);

            // asserts
            Assert.IsFalse(found);
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
