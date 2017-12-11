using System;
using System.Collections.Generic;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using NLog;
using System.Reflection;

namespace Messenger.DataLayer.Sql
{
    // TODO: Exact information saying why validation failed.
    // TODO: Bigger test coverage
    public class MessageRepository : IMessageRepository
    {
        private readonly string connectionString;
        private readonly MessageRepositoryInputValidator validator;
        private readonly UserRepository userRepository;
        private static readonly Logger logger = LogManager.GetLogger(typeof(UserRepository).FullName);

        public MessageRepository(string connectionString, UserRepository userRepository)
        {
            this.connectionString = connectionString;
            this.userRepository = userRepository;
            validator = new MessageRepositoryInputValidator(
                userRepository, 
                new ChatRepository(connectionString, userRepository),
                new AttachmentRepository(connectionString));
        }



        /// <summary>
        /// Saves message to database.
        /// </summary>
        /// <param name="message"> 
        /// New message data.
        /// - Id is given by system as Guid.New(). 
        /// - Date is given by system as DateTime.Now. 
        /// - At least one field of these to should be filled: Text or AttachmentId.
        /// - Other fields required to be filled by caller. 
        /// </param>
        /// <returns> 
        /// Created message: success
        /// Null: invalid input
        /// Throws SqlException: problems with database
        /// </returns>
        public Message Send(Message message)
        {
            logger.Info($"Attempting to send message. " +
                        $"ChatId = [{message?.ChatId}], " +
                        $"AuthorId = [{message?.AuthorId}], " +
                        $"Text = [{message?.Text}], " +
                        $"AttachmentId = [{message?.AttachmentId}], " + 
                        $"SelfDeletion = [{message?.SelfDeletion.ToString()}]");

            bool valid = false;
            try { valid = validator.ValidateMessageInput(message); }
            catch (SqlException ex)
            {
                var messageData = $"ChatId = [{message?.ChatId}], " +
                                  $"AuthorId = [{message?.AuthorId}], " +
                                  $"Text = [{message?.Text}], " +
                                  $"AttachmentId = [{message?.AttachmentId}], " +
                                  $"SelfDeletion = [{message?.SelfDeletion.ToString()}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), messageData, logger);
            }
            if (!valid)
            {
                logger.Info($"Failed to send message. Invalid data. " +
                            $"ChatId = [{message?.ChatId}], " +
                            $"AuthorId = [{message?.AuthorId}], " +
                            $"Text = [{message?.Text}], " +
                            $"AttachmentId = [{message?.AttachmentId}], " +
                            $"SelfDeletion = [{message?.SelfDeletion.ToString()}]");

                return null;
            }

            AddUserData(message);

            try { SendMessageQuery(message); }
            catch (SqlException ex)
            {
                var messageData = $"ChatId = [{message.ChatId}], " +
                                  $"AuthorId = [{message.AuthorId}], " +
                                  $"Text = [{message.Text}], " +
                                  $"AttachmentId = [{message.AttachmentId}], " +
                                  $"SelfDeletion = [{message.SelfDeletion.ToString()}]";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), messageData, logger);
            }

            logger.Info($"Message successfully sent. " +
                        $"Id = [{message.Id}], " +
                        $"ChatId = [{message.ChatId}], " +
                        $"AuthorId = [{message.AuthorId}], " +
                        $"Date = [{message.Date}], " +
                        $"Text = [{message.Text}], " +
                        $"AttachmentId = [{message.AttachmentId}], " +
                        $"SelfDeletion = [{message.SelfDeletion.ToString()}]");

            return message;
        }

        /// <summary>
        /// Gets message by id.
        /// </summary>
        /// <param name="messageId"> Message id </param>
        /// <returns> 
        /// Found message data: success
        /// Null: message not found 
        /// Throws SqlException: problems with database
        /// </returns>
        public Message Get(Guid messageId)
        {
            logger.Info($"Attempting to find message. Id = [{messageId}]");

            Message foundMessage = null;

            try { foundMessage = GetMessage(messageId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{messageId}]", logger);
            }
            if (foundMessage == null)
            {
                logger.Info($"Message not found. Id = [{messageId}]");
                return null;
            }

            logger.Info($"Message found. " +
                        $"Id = [{foundMessage.Id}], " +
                        $"ChatId = [{foundMessage.ChatId}], " +
                        $"AuthorId = [{foundMessage.AuthorId}], " +
                        $"Date = [{foundMessage.Date}], " +
                        $"Text = [{foundMessage.Text}], " +
                        $"AttachmentId = [{foundMessage.AttachmentId}], " +
                        $"SelfDeletion = [{foundMessage.SelfDeletion.ToString()}]");

            return foundMessage;
        }

        /// <summary>
        /// Gets chat messages.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <returns>
        /// Found chat messages: success
        /// Throws SqlException: problems with database
        /// </returns>
        public ReadOnlyCollection<Message> GetChatMessages(Guid chatId)
        {
            logger.Info($"Attempting to find chat messages. chatId = [{chatId}]");

            ReadOnlyCollection<Message> chatMessages = null;

            try { chatMessages = GetChatMessagesAction(chatId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"chatId = [{chatId}]", logger);
            }

            logger.Info($"Chat messages found. Count = [{chatMessages.Count}]");
            return chatMessages;
        }

        /// <summary>
        /// Gets new messages, depending currentNumberOfFetchedMessages and number of messages in database.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <param name="currentNumberOfFetchedMessages"> 
        /// Number of messages that caller currently has. Would be substracted by number 
        /// of messages of given chat currently in database.
        /// Difference would tell how many messages should be sent back. 
        /// </param>
        /// <returns>
        /// Found chat messages: success
        /// Throws SqlException: problems with database
        /// </returns>
        public ReadOnlyCollection<Message> GetNewChatMessages(Guid chatId, int currentNumberOfFetchedMessages)
        {
            logger.Info($"Attempting to find new chat messages. chatId = [{chatId}], " +
                        $"currentNumberOfFetchedMessages = [{currentNumberOfFetchedMessages}]");

            ReadOnlyCollection<Message> chatMessages = null;

            try { chatMessages = GetNewChatMessagesAction(chatId, currentNumberOfFetchedMessages); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"chatId = [{chatId}], " +
                    $"currentNumberOfFetchedMessages = [{currentNumberOfFetchedMessages}]", logger);
            }
            if (chatMessages == null) chatMessages = new ReadOnlyCollection<Message>(new List<Message>()); 

            logger.Info($"Chat new messages found. chatId = [{chatId}], " +
                        $"currentNumberOfFetchedMessages = [{currentNumberOfFetchedMessages}]");

            return chatMessages;
        }

        /// <summary>
        /// Deletes message with given id. 
        /// </summary>
        /// <param name="messageId"> Message id </param>
        /// <returns>
        /// True: message deleted
        /// False: message not found
        /// Throws SqlException: problems with database
        /// </returns>
        public bool Delete(Guid messageId)
        {
            logger.Info($"Attempting to delete message. Id = [{messageId}]");

            Message foundMessage = null;

            try { foundMessage = Get(messageId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{messageId}]", logger);
            }
            if (foundMessage == null)
            {
                logger.Info($"Unable to delete message. Id = [{messageId}]");
                return false;
            }

            try { DeleteMessageQuery(messageId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{messageId}]", logger);
            }

            logger.Info($"Message successfully deleted. Id = [{messageId}]");
            return true;
        }



        /// <summary>
        /// Gives message unique Id and sets date and time of sending.
        /// </summary>
        /// <param name="message"> Message data </param>
        private void AddUserData(Message message)
        {
            message.Id = Guid.NewGuid();
            // date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
            message.Date = DateTime.Now;
        }

        private void SendMessageQuery(Message message)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                            "INSERT INTO [Message] ([Id], [ChatId], [AuthorId], [Date], [Text], [AttachmentId], [SelfDeletion]) " +
                            "VALUES (@id, @chatId, @authorId, @date, @text, @attachmentId, @selfDelition)";

                    command.Parameters.AddWithValue("@id", message.Id);
                    command.Parameters.AddWithValue("@chatId", message.ChatId);
                    command.Parameters.AddWithValue("@authorId", message.AuthorId);
                    command.Parameters.AddWithValue("@date", message.Date);
                    command.Parameters.AddWithValue("@text", message.Text ?? (object)DBNull.Value);
                    if (message.AttachmentId == Guid.Empty) command.Parameters.AddWithValue("@attachmentId", DBNull.Value);
                    else command.Parameters.AddWithValue("@attachmentId", message.AttachmentId);
                    command.Parameters.AddWithValue("@selfDelition", message.SelfDeletion);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Message GetMessage(Guid messageId)
        {
            Message foundMessage = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetMessageQuery(command, messageId);
                    foundMessage = RetrieveMessageData(command);
                }
            }

            return foundMessage;
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

        private Message RetrieveMessageData(SqlCommand command)
        {
            Message message = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    message = new Message
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        ChatId = reader.GetGuid(reader.GetOrdinal("ChatId")),
                        AuthorId = reader["AuthorId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AuthorId")),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Text = reader["Text"] == DBNull.Value ? null : reader.GetString(reader.GetOrdinal("Text")),
                        AttachmentId = reader["AttachmentId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AttachmentId")),
                        SelfDeletion = reader.GetBoolean(reader.GetOrdinal("SelfDeletion"))
                    };
                }
            }

            return message;
        }

        private ReadOnlyCollection<Message> GetChatMessagesAction(Guid chatId)
        {
            ReadOnlyCollection<Message> chatMessages = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetChatMessagesQuery(command, chatId);
                    chatMessages = RetrieveMessagesData(command);
                }
            }

            return chatMessages;
        }

        private void GetChatMessagesQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Message]" +
                "WHERE [ChatId] = @chatId";
            command.Parameters.AddWithValue("@chatId", chatId);

            command.ExecuteNonQuery();
        }

        private ReadOnlyCollection<Message> RetrieveMessagesData(SqlCommand command)
        {
            var messages = new List<Message>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    messages.Add(new Message
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        ChatId = reader.GetGuid(reader.GetOrdinal("ChatId")),
                        AuthorId = reader["AuthorId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AuthorId")),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Text = reader["Text"] == DBNull.Value ? null : reader.GetString(reader.GetOrdinal("Text")),
                        AttachmentId = reader["AttachmentId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AttachmentId")),
                        SelfDeletion = reader.GetBoolean(reader.GetOrdinal("SelfDeletion"))
                    });
                }
            }

            return messages.AsReadOnly();
        }

        private ReadOnlyCollection<Message> GetNewChatMessagesAction(Guid chatId, int currentNumberOfFetchedMessages)
        {
            ReadOnlyCollection<Message> chatMessages = null;
            int numberOfMessagesInChat = 0;
            int numberOfNewMessages = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        GetNumberOfNewMessagesQuery(command, chatId);
                        numberOfMessagesInChat = RetrieveNumberOfNewMessages(command);
                    }

                    numberOfNewMessages = numberOfMessagesInChat - currentNumberOfFetchedMessages;
                    if (numberOfNewMessages == 0) return chatMessages;

                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        GetNewChatMessagesQuery(command, chatId, numberOfNewMessages);
                        chatMessages = RetrieveMessagesData(command);
                    }

                    transaction.Commit();
                }
            }

            return chatMessages;
        }

        private void GetNumberOfNewMessagesQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT COUNT(*) AS NumberOfMessagesInChat " +
                "FROM [Message] " +
                "WHERE [ChatId] = @chatId";
            command.Parameters.AddWithValue("@chatId", chatId);

            command.ExecuteNonQuery();
        }

        private int RetrieveNumberOfNewMessages(SqlCommand command)
        {
            int numberOfMessagesInChat = 0;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    numberOfMessagesInChat = reader.GetInt32(reader.GetOrdinal("NumberOfMessagesInChat"));
                }
            }

            return numberOfMessagesInChat;
        }

        private void GetNewChatMessagesQuery(SqlCommand command, Guid chatId, int numberOfNewMessages)
        {
            command.CommandText =
                "SELECT TOP (@numberOfNewMessages) * " +
                "FROM [Message] " +
                "WHERE [ChatId] = @chatId " +
                "ORDER BY [Date] DESC";
            command.Parameters.AddWithValue("@numberOfNewMessages", numberOfNewMessages);
            command.Parameters.AddWithValue("@chatId", chatId);

            command.ExecuteNonQuery();
        }

        private void DeleteMessageQuery(Guid messageId)
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
    }
}
