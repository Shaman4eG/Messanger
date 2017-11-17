using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Model;
using System.Data.SqlClient;

namespace Messenger.DataLayer.Sql
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly string connectionString;
        private readonly AttachmentRepositoryInputValidator validator;

        public AttachmentRepository(string connectionString)
        {
            this.connectionString = connectionString;
            validator = new AttachmentRepositoryInputValidator();
        }



        public Attachment Create(Attachment attachment)
        {
            bool valid = validator.ValidateInput(attachment);
            if (!valid) return null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    CreateAttachmentQuery(command, attachment);
                }
            }

            return attachment;
        }

        public void Delete(Guid attachmentId)
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

        public Attachment Get(Guid atttachmentId)
        {
            Attachment foundAttachment = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetAttachmentQuery(command, atttachmentId);
                    foundAttachment = PutAttachmentDataInAttachment(command);
                }
            }

            return foundAttachment;
        }



        private void CreateAttachmentQuery(SqlCommand command, Attachment attachment)
        {
            command.CommandText =
                "INSERT INTO [Attachment] ([Id], [Type], [File]) " +
                "VALUES (@id, @type, @file)";

            attachment.Id = Guid.NewGuid();
            command.Parameters.AddWithValue("@id", attachment.Id);
            command.Parameters.AddWithValue("@type", attachment.Type);
            command.Parameters.AddWithValue("@file", attachment.File);

            command.ExecuteNonQuery();
        }

        private void GetAttachmentQuery(SqlCommand command, Guid id)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Attachment] " +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
        }

        private Attachment PutAttachmentDataInAttachment(SqlCommand command)
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
    }
}
