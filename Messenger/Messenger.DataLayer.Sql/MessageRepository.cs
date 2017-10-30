using System;
using System.Collections.Generic;
using Messenger.DataLayer;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;

namespace Messenger.DataLayer.Sql
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessageRepositoryInputValidator validator;
        private readonly string connectionString;
        private readonly UserRepository userRepository;

        public MessageRepository(string connectionString, UserRepository userRepository)
        {
            validator = new MessageRepositoryInputValidator();
            this.connectionString = connectionString;
            this.userRepository = userRepository;
        }



        public Message Send(Message message)
        {
            bool valid = validator.ValidateSend(message);
            if (!valid) return null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SendMessageQuery(connection, message);
            }

            return message;
        }

        public void Delete(Guid messageId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM [Message]" +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", messageId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public Message Get(Guid messageId)
        {
            Message message = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetMessageQuery(command, messageId);
                    message = PutMessageDataInMessage(command);
                }
            }

            return message;
        }

        public ReadOnlyCollection<Message> GetChatMessages(Guid chatId)
        {
            ReadOnlyCollection<Message> chatMessages;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetChatMessagesQuery(command, chatId);
                    chatMessages = RetrieveChatMessages(command);
                }
            }

            return chatMessages;
        }



        private void SendMessageQuery(SqlConnection connection, Message message)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                        "INSERT INTO [Message] ([Id], [ChatId], [AuthorId], [Date], [Text], [AttachmentId], [SelfDeletion]) " +
                        "VALUES (@id, @chatId, @authorId, @date, @text, @attachmentId, @selfDelition)";
                message.Id = Guid.NewGuid();
                command.Parameters.AddWithValue("@id", message.Id);
                command.Parameters.AddWithValue("@chatId", message.ChatId);
                command.Parameters.AddWithValue("@authorId", message.Author.Id);
                command.Parameters.AddWithValue("@date", message.Date);
                command.Parameters.AddWithValue("@text", message.Text ?? (object)DBNull.Value);
                if (message.AttachmentId == Guid.Empty) command.Parameters.AddWithValue("@attachmentId", DBNull.Value);
                else command.Parameters.AddWithValue("@attachmentId", message.AttachmentId);
                command.Parameters.AddWithValue("@selfDelition", message.SelfDeletion);

                command.ExecuteNonQuery();
            }
        }

        private void GetMessageQuery(SqlCommand command, Guid messageId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Message]" +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", messageId);

            command.ExecuteNonQuery();
        }

        private Message PutMessageDataInMessage(SqlCommand command)
        {
            Message message = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    message = new Message
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        ChatId = reader["ChatId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("ChatId")),
                        Author = userRepository.Get(reader.GetGuid(reader.GetOrdinal("AuthorId"))),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Text = reader["Text"] == DBNull.Value ? null : reader.GetString(reader.GetOrdinal("Text")),
                        AttachmentId = reader["AttachmentId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AttachmentId")),
                        SelfDeletion = reader.GetBoolean(reader.GetOrdinal("SelfDeletion"))
                    };
                }
            }

            return message;
        }

        private void GetChatMessagesQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT [Id] " +
                "FROM [Message]" +
                "WHERE [ChatId] = @chatId";
            command.Parameters.AddWithValue("@chatId", chatId);

            command.ExecuteNonQuery();
        }
        
        private ReadOnlyCollection<Message> RetrieveChatMessages(SqlCommand command)
        {
            var chatMessages = new List<Message>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var message = Get(reader.GetGuid(reader.GetOrdinal("Id")));
                    chatMessages.Add(message);
                }
            }

            return chatMessages.AsReadOnly();
        }
    }
}
