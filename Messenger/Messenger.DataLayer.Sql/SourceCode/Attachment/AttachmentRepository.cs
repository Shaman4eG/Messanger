using System;
using Messenger.Model;
using System.Data.SqlClient;
using NLog;
using System.Net.Http;
using System.Web.Http;
using System.Reflection;

namespace Messenger.DataLayer.Sql
{
    /// <summary>
    /// Provides methods for manipulations on Attachment entity.
    /// </summary>
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly string connectionString;
        private readonly AttachmentRepositoryInputValidator validator = new AttachmentRepositoryInputValidator();
        private static readonly Logger logger = LogManager.GetLogger(typeof(AttachmentRepository).FullName);

        public AttachmentRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }



        /// <summary>
        /// Creates attachment with provided data and saves it to database.
        /// </summary>
        /// <param name="attachment">
        /// New attachment data.
        /// - Id is given by system as Guid.New().  
        /// - Other fields required to be filled by caller.
        /// </param>
        /// <returns>
        /// Created attachment: success 
        /// Null: invalid input
        /// Throws SqlException: problems with database
        /// </returns>
        public Attachment Create(Attachment attachment)
        {
            logger.Info($"Attempting to create attachment. Type = [{attachment?.Type}]");

            bool valid = validator.ValidateAttachmentInput(attachment);
            if (!valid)
            {
                logger.Info($"Failed to create attachment. Invalid data. Type = [{attachment?.Type}]");
                return null;
            }

            AddAttachmentData(attachment);

            try { CreateAttachmentQuery(attachment); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Type = [{attachment.Type}]", logger);
            }

            logger.Info($"Attachment successfully created. " +
                        $"Id = [{attachment.Id}], " +
                        $"Type = [{attachment.Type}]");

            return attachment;
        }

        /// <summary>
        /// Gets attachment by id.
        /// </summary>
        /// <param name="attachmentId"> Attachment id </param>
        /// <returns> 
        /// Found attachment data: success 
        /// Null: attachment not found 
        /// Throws SqlException: problems with database
        /// </returns>
        public Attachment Get(Guid attachmentId)
        {
            logger.Info($"Attempting to find attachment. Id = [{attachmentId}]");

            Attachment foundAttachment = null;

            try { foundAttachment = GetAttachment(attachmentId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{attachmentId}]", logger);
            }
            if (foundAttachment == null)
            {
                logger.Info($"Attachment not found. Id = [{attachmentId}]");
                return null;
            }

            logger.Info($"Attachment found. " +
                        $"Id = [{foundAttachment.Id}], " +
                        $"Type = [{foundAttachment.Type}]");

            return foundAttachment;
        }

        /// <summary>
        /// Deletes attachment with given id.
        /// </summary>
        /// <param name="attachmentId"> Attachment id </param>
        /// <returns>
        /// True: attachment deleted
        /// False: attachment not found
        /// Throws SqlException: problems with database
        /// </returns>
        public bool Delete(Guid attachmentId)
        {
            logger.Info($"Attempting to delete attachment. Id = [{attachmentId}]");

            Attachment foundAttachment = null;

            try { foundAttachment = Get(attachmentId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{attachmentId}]", logger);
            }
            if (foundAttachment == null)
            {
                logger.Info($"Unable to delete attachment. Id = [{attachmentId}]");
                return false;
            }

            try { DeleteAttachmentQuery(attachmentId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{attachmentId}]", logger);
            }

            logger.Info($"Attachment successfully deleted. Id = [{attachmentId}]");
            return true;
        }



        /// <summary>
        /// Gives attachment unique Id.
        /// </summary>
        /// <param name="attachment"> Attachment data </param>
        private void AddAttachmentData(Attachment attachment)
        {
            attachment.Id = Guid.NewGuid();
        }

        private void CreateAttachmentQuery(Attachment attachment)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [Attachment] ([Id], [Type], [File]) " +
                        "VALUES (@id, @type, @file)";

                    command.Parameters.AddWithValue("@id", attachment.Id);
                    command.Parameters.AddWithValue("@type", attachment.Type);
                    command.Parameters.AddWithValue("@file", attachment.File);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Attachment GetAttachment(Guid attachmentId)
        {
            Attachment foundAttachment = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetAttachmentQuery(command, attachmentId);
                    foundAttachment = RetrieveAttachmentData(command);
                }
            }

            return foundAttachment;
        }

        private void GetAttachmentQuery(SqlCommand command, Guid attachmentId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Attachment] " +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", attachmentId);

            command.ExecuteNonQuery();
        }

        private Attachment RetrieveAttachmentData(SqlCommand command)
        {
            Attachment attachment = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    attachment = new Attachment
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Type = reader.GetString(reader.GetOrdinal("Type")),
                        File = (byte[])reader["File"]
                    };
                }
            }

            return attachment;
        }

        private void DeleteAttachmentQuery(Guid attachmentId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM [Attachment]" +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", attachmentId);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
