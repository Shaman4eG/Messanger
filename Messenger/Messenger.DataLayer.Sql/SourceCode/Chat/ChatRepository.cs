using System;
using System.Collections.Generic;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using NLog;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using Messenger.Constants;
using System.Reflection;

namespace Messenger.DataLayer.Sql
{
    // TODO: Add Update()
    // TODO: Exact information saying why validation failed.
    // TODO: Bigger test coverage
    /// <summary>
    /// Provides methods for manipulations on Chat entity.
    /// </summary>
    public class ChatRepository : IChatRepository
    {
        private readonly string connectionString;
        private readonly UserRepository userRepository;
        private readonly ChatRepositoryInputValidator validator;
        private static readonly Logger logger = LogManager.GetLogger(typeof(ChatRepository).FullName);

        public ChatRepository(string connectionString, UserRepository userRepository)
        {
            this.connectionString = connectionString;
            this.userRepository = userRepository;
            validator = new ChatRepositoryInputValidator(
                connectionString, 
                new AttachmentRepository(connectionString),
                userRepository, 
                this);
        }



        /// <summary>
        /// Creates chat with provided data and saves it to database.
        /// </summary>
        /// <param name="chat">
        /// New chat data.
        /// - Id is given by system as Guid.New(). 
        /// - Type is given by system depending on number of members: 1 or 2 = personal, 3 and more = group.
        /// - AvataId is optional and can be specified only in group chat.
        /// - If chat type is group, caller should specify AdminId and Name.
        /// - List of members is required.
        /// </param>
        /// <returns>
        /// Created chat: success
        /// Null: invalid input
        /// Throws SqlException: problems with database
        /// </returns>
        public Chat Create(Chat chat) 
        {
            logger.Info($"Attempting to create chat. " +
                        $"AdminId = [{chat?.AdminId}], " +
                        $"Name = [{chat?.Name}], " +
                        $"AvatarId = [{chat?.AvatarId}], " +
                        $"{RepresentChatMembersAsStringOfIds(chat)}");

            bool valid = false;
            try { valid = validator.ValidateChatInputAndAddData(chat); }
            catch (SqlException ex)
            {
                var chatData = $"AdminId = [{chat?.AdminId}], " +
                               $"Name = [{chat?.Name}], " +
                               $"AvatarId = [{chat?.AvatarId}], " +
                               $"{RepresentChatMembersAsStringOfIds(chat)}";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), chatData, logger);
            }
            if (!valid)
            {
                logger.Info($"Failed to create chat. Invalid data. " +
                            $"AdminId = [{chat?.AdminId}], " +
                            $"Name = [{chat?.Name}], " +
                            $"AvatarId = [{chat?.AvatarId}], " +
                            $"{RepresentChatMembersAsStringOfIds(chat)}");

                return null;
            }

            try { CreateChat(chat); }
            catch (SqlException ex)
            {
                var chatData = $"AdminId = [{chat.AdminId}], " +
                               $"Name = [{chat.Name}], " +
                               $"AvatarId = [{chat.AvatarId}], " +
                               $"{RepresentChatMembersAsStringOfIds(chat)}";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), chatData, logger);
            }

            logger.Info($"Chat successfully created. " +
                        $"Id = [{chat.Id}], " +
                        $"Type = [{chat.Type}], " +
                        $"AdminId = [{chat.AdminId}], " +
                        $"Name = [{chat.Name}], " +
                        $"AvatarId = [{chat.AvatarId}], " +
                        $"{RepresentChatMembersAsStringOfIds(chat)}");

            return chat;
        }

        /// <summary>
        /// Gets chat by id.
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <returns>
        /// Found chat data: success
        /// Null: chat not found 
        /// Throws SqlException: problems with database
        /// </returns>
        public Chat Get(Guid chatId)
        {
            logger.Info($"Attempting to find chat. Id = [{chatId}]");

            Chat foundChat = null;

            try { foundChat = GetChat(chatId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{chatId}]", logger);
            }
            if (foundChat == null)
            {
                logger.Info($"Chat not found. Id = [{chatId}]");
                return null;
            }

            logger.Info($"Chat found. " +
                        $"Id = [{foundChat.Id}], " +
                        $"Type = [{foundChat.Type}], " +
                        $"AdminId = [{foundChat.AdminId}], " +
                        $"Name = [{foundChat.Name}], " +
                        $"AvatarId = [{foundChat.AvatarId}], " +
                        $"{RepresentChatMembersAsStringOfIds(foundChat)}");

            return foundChat;
        }

        // TODO: Add user existens check.
        /// <summary>
        /// Gets user chats.
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns>
        /// Found user chats: success
        /// Null: user chats not found 
        /// Throws SqlException: problems with database
        /// </returns>
        public ReadOnlyCollection<Chat> GetUserChats(Guid userId)
        {
            logger.Info($"Attempting to find user chats. userId = [{userId}]");

            ReadOnlyCollection<Chat> userChats = null;

            try { userChats = GetUserChatsAction(userId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"userId = [{userId}]", logger);
            }

            logger.Info($"User chats found. Count = [{userChats.Count}]");
            return userChats;
        }

        /// <summary>
        /// Deletes chat with given id. 
        /// </summary>
        /// <param name="chatId"> Chat id </param>
        /// <returns>
        /// True: chat deleted
        /// False: chat not found
        /// Throws SqlException: problems with database
        /// </returns>
        public bool Delete(Guid chatId)
        {
            logger.Info($"Attempting to delete chat. Id = [{chatId}]");

            Chat foundChat = null;

            try { foundChat = Get(chatId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{chatId}]", logger);
            }
            if (foundChat == null)
            {
                logger.Info($"Unable to delete chat. Id = [{chatId}]");
                return false;
            }

            try { DeleteChatQuery(chatId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{chatId}]", logger);
            }

            logger.Info($"Chat successfully deleted. Id = [{chatId}]");
            return true;
        }



        internal string RepresentChatMembersAsStringOfIds(Chat chat)
        {
            var chatData = new StringBuilder("Members = ");

            if (chat?.Members == null)
            {
                chatData.Append("[null]");
                return chatData.ToString();
            }

            foreach (User member in chat.Members)
            {
                chatData.Append($"[{member.Id}], ");
            }
            // Removing whitespace and comma after last id.
            chatData.Remove(chatData.Length - 2, 2);
            
            return chatData.ToString();
        }



        private void CreateChat(Chat chat)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    CreateChatQuery(connection, transaction, chat);
                    BoundChatMembersAndChatQuery(connection, transaction, chat);

                    transaction.Commit();
                }
            }
        }

        private void CreateChatQuery(
            SqlConnection connection,
            SqlTransaction transaction,
            Chat chat)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText =
                        "INSERT INTO [Chat] ([Id], [Type], [AdminId], [Name], [AvatarId]) " +
                        "VALUES (@id, @type, @adminId, @name, @avatarId)";
                command.Parameters.AddWithValue("@id", chat.Id);
                command.Parameters.AddWithValue("@type", chat.Type.ToString());
                command.Parameters.AddWithValue("@adminId", chat.AdminId == Guid.Empty ? (object)DBNull.Value : chat.AdminId);
                command.Parameters.AddWithValue("@name", chat.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@avatarId", chat.AvatarId == Guid.Empty ? (object)DBNull.Value : chat.AvatarId);

                command.ExecuteNonQuery();
            }
        }

        private void BoundChatMembersAndChatQuery(
            SqlConnection connection,
            SqlTransaction transaction,
            Chat chat)
        {
            foreach (var user in chat.Members)
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText =
                            "INSERT INTO [ChatMembers] ([ChatId], [UserId]) " +
                            "VALUES (@chatId, @userId)";
                    command.Parameters.AddWithValue("@chatId", chat.Id);
                    command.Parameters.AddWithValue("@userId", user.Id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Chat GetChat(Guid chatId)
        {
            Chat foundChat = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    GetChatQuery(command, chatId);
                    foundChat = RetrieveChatData(command);
                }

                // Chat not found
                if (foundChat == null) return null;

                using (var command = connection.CreateCommand())
                {
                    GetChatMembersQuery(command, chatId);
                    foundChat.Members = RetrieveChatMembers(command);
                }
            }

            return foundChat;
        }

        private void GetChatQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Chat]" +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", chatId);

            command.ExecuteNonQuery();
        }

        private Chat RetrieveChatData(SqlCommand command)
        {
            Chat chat = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chat = new Chat
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString(reader.GetOrdinal("Type"))),
                        AdminId = reader["AdminId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AdminId")),
                        Name = reader["Name"] == DBNull.Value ? null : reader.GetString(reader.GetOrdinal("Name")),
                        AvatarId = reader["AvatarId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AvatarId"))
                    };
                }
            }

            return chat;
        }
       
        private void GetChatMembersQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT [UserId] " +
                "FROM [ChatMembers]" +
                "WHERE [ChatId] = @chatId";
            command.Parameters.AddWithValue("@chatId", chatId);

            command.ExecuteNonQuery();
        }

        private ReadOnlyCollection<User> RetrieveChatMembers(SqlCommand command)
        {
            var members = new List<User>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var chatMember = userRepository.GetById(reader.GetGuid(reader.GetOrdinal("UserId"))); 
                    members.Add(chatMember);
                }
            }

            return members.AsReadOnly(); 
        }

        private ReadOnlyCollection<Chat> GetUserChatsAction(Guid userId)
        {
            ReadOnlyCollection<Chat> userChats = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    GetUserChatsQuery(command, userId);
                    userChats = RetrieveUserChats(command);
                }
            }

            return userChats;
        }

        private void GetUserChatsQuery(SqlCommand command, Guid userId)
        {
            command.CommandText =
                "SELECT [ChatId] " +
                "FROM [ChatMembers]" +
                "WHERE [UserId] = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            command.ExecuteNonQuery();
        }

        private ReadOnlyCollection<Chat> RetrieveUserChats(SqlCommand command)
        {
            var userChats = new List<Chat>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var chat = Get(reader.GetGuid(reader.GetOrdinal("ChatId")));
                    userChats.Add(chat);
                }
            }

            return userChats.AsReadOnly();
        }

        private void DeleteChatQuery(Guid chatId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM [Chat]" +
                        "WHERE [Id] = @id";
                    command.Parameters.AddWithValue("@id", chatId);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
