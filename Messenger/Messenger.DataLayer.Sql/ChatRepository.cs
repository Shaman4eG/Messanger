using System;
using System.Collections.Generic;
using Messenger.DataLayer;
using Messenger.Model;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Messenger.DataLayer.Sql
{
    public class ChatRepository : IChatRepository
    {
        private readonly string connectionString;
        private readonly UserRepository userRepository;

        private readonly int MaxMembersInChat = 10;

        public ChatRepository(string connectionString, UserRepository userRepository)
        {
            this.connectionString = connectionString;
            this.userRepository = userRepository;
        }



        public Chat Create(ReadOnlyCollection<Guid> members, Guid adminId, string name = "", byte[] avatar = null) 
        {
            Chat chat = null;

            // TEST IT
            try
            {
                chat = InitializeChat(adminId, members, name, avatar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                                "INSERT INTO [Chat] ([Id], [Type], [AdminId], [Name], [Avatar]) " +
                                "VALUES (@id, @type, @adminId, @name, @avatar)";
                        command.Parameters.AddWithValue("@id", chat.Id);
                        command.Parameters.AddWithValue("@type", chat.Type.ToString());
                        command.Parameters.AddWithValue("@adminId", chat.AdminId);
                        command.Parameters.AddWithValue("@name", chat.Name);
                        command.Parameters.AddWithValue("@avatar", chat.Avatar);

                        command.ExecuteNonQuery();
                    }

                    foreach (var userId in members)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText =
                                    "INSERT INTO [UsersInChat] ([ChatId], [UserId]) " +
                                    "VALUES (@chatId, @userId)";
                            command.Parameters.AddWithValue("@chatId", chat.Id);
                            command.Parameters.AddWithValue("@userId", userId);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    var usersInChat = new List<User>();
                    foreach (var userId in members)
                        usersInChat.Add(userRepository.Get(userId));
                    chat.Members = usersInChat.AsReadOnly();

                    return chat;
                }
            }
        }

        private Chat InitializeChat(Guid adminId, ReadOnlyCollection<Guid> members, string name, byte[] avatar)
        {
            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                Type = DefineChatType(members),
                Avatar = DefineAvatar(avatar)
            };
            chat.AdminId = DefineAdminId(adminId, chat.Type, members);
            chat.Name = DefineChatName(name, chat.Type, members);

            return chat;
        }

        // TODO: WHY NULL DOESNT WORK?
        private byte[] DefineAvatar(byte[] avatar)
        {
            if (avatar != null) return avatar;

            return new byte[0];
        }

        // TODO: Change name of personal chat to person's name.
        private string DefineChatName(string name, ChatType type, ReadOnlyCollection<Guid> members)
        {
            if (type == ChatType.personal) return "Pesonal Chat";

            if (name != "" && name != null) return name;
            else return "Group Chat";
        }

        private ChatType DefineChatType(ReadOnlyCollection<Guid> members)
        {
            if (members == null) throw new Exception("No members in chat.");
            if (members.Count > MaxMembersInChat) throw new Exception("Too many chat members. Max = " + MaxMembersInChat);

            return members.Count > 2 ? ChatType.group : ChatType.personal;
        }

        // TODO: create own Exception class
        private Guid DefineAdminId(Guid adminId, ChatType type, ReadOnlyCollection<Guid> members)
        {
            if (type == ChatType.personal) return members[0];

            if (members.Contains(adminId)) return adminId;
            else throw new Exception("Admin is not a chat member.");
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
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "SELECT * " +
                            "FROM [Chat]" +
                            "WHERE [Id] = @id";
                        command.Parameters.AddWithValue("@id", chatId);

                        command.ExecuteNonQuery();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                chat = new Chat
                                {
                                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                    Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString(reader.GetOrdinal("Type"))),
                                    AdminId = reader.GetGuid(reader.GetOrdinal("AdminId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Avatar = (byte[])reader["Avatar"],
                                };
                            }
                        }
                    }

                    if (chat == null) return null;

                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "SELECT [UserId] " +
                            "FROM [UsersInChat]" +
                            "WHERE [ChatId] = @chatId";
                        command.Parameters.AddWithValue("@chatId", chatId);

                        command.ExecuteNonQuery();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<User> members = new List<User>();
                            while (reader.Read())
                            {
                                members.Add(userRepository.Get(reader.GetGuid(reader.GetOrdinal("UserId"))));
                            }
                            chat.Members = members.AsReadOnly();
                        }
                    }

                    transaction.Commit();
                }

            }

            return chat;
        }



        public IEnumerable<Chat> GetUserChats(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
