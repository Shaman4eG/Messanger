using System;
using System.Collections.Generic;
using Messenger.DataLayer;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;

namespace Messenger.DataLayer.Sql
{
    public class ChatRepository : IChatRepository
    {
        private readonly string connectionString;
        private readonly UserRepository userRepository;
        private readonly ChatRepositoryInputValidator validator;

        public ChatRepository(string connectionString, UserRepository userRepository)
        {
            this.connectionString = connectionString;
            this.userRepository = userRepository;
            validator = new ChatRepositoryInputValidator();
        }



        public Chat Create(Chat chat) 
        {
            bool valid = validator.ValidateCreateAndAddInfo(chat);
            if (!valid) return null;

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

            return chat;
        }

        public void Delete(Guid chatId)
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

        public Chat Get(Guid chatId)
        {
            Chat chat = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    GetChatQuery(command, chatId);
                    chat = PutChatDataInChat(command);
                }

                // Chat not found
                if (chat == null) return null;

                using (var command = connection.CreateCommand())
                {
                    GetChatMembersQuery(command, chatId);
                    PutChatMembersInChat(command, chat);
                }
            }

            return chat;
        }

        public ReadOnlyCollection<Chat> GetUserChats(Guid userId)
        {
            ReadOnlyCollection<Chat> userChats;
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



        private void CreateChatQuery(
            SqlConnection connection,
            SqlTransaction transaction,
            Chat chat)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                                "INSERT INTO [Chat] ([Id], [Type], [AdminId], [Name], [Avatar]) " +
                                "VALUES (@id, @type, @adminId, @name, @avatar)";
                        command.Parameters.AddWithValue("@id", chat.Id);
                        command.Parameters.AddWithValue("@type", chat.Type.ToString());
                        if (chat.AdminId == Guid.Empty) command.Parameters.AddWithValue("@adminId", DBNull.Value);
                        else command.Parameters.AddWithValue("@adminId", chat.AdminId);
                        command.Parameters.AddWithValue("@name", chat.Name ?? (object)DBNull.Value);
                        var avatar = new SqlParameter("@avatar", SqlDbType.Image);
                        avatar.Value = chat.Avatar ?? (object)DBNull.Value;
                        command.Parameters.Add(avatar);

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

        private void GetChatQuery(SqlCommand command, Guid chatId)
        {
            command.CommandText =
                "SELECT * " +
                "FROM [Chat]" +
                "WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", chatId);

            command.ExecuteNonQuery();
        }

        private Chat PutChatDataInChat(SqlCommand command)
        {
            Chat chat = null;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    chat = new Chat
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString(reader.GetOrdinal("Type"))),
                        AdminId = reader["AdminId"] == DBNull.Value ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("AdminId")),
                        Name = reader["Name"] == DBNull.Value ? null : reader.GetString(reader.GetOrdinal("Name")),
                        Avatar = reader["Avatar"] == DBNull.Value ? null : (byte[])reader["Avatar"]
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

        private void PutChatMembersInChat(SqlCommand command, Chat chat)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                List<User> members = new List<User>();
                while (reader.Read())
                {
                    var chatMemberId = reader.GetGuid(reader.GetOrdinal("UserId"));
                    var chatMember = userRepository.Get(chatMemberId);
                    members.Add(chatMember);
                }
                chat.Members = members.AsReadOnly();
            }
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
    }
}
