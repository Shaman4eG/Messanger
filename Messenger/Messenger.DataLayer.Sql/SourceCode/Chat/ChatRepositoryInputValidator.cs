using System;
using Messenger.Model;
using System.Data.SqlClient;
using NLog;
using System.Web.Http;
using System.Collections.Generic;
using Messenger.Constants;
using System.Reflection;

namespace Messenger.DataLayer.Sql
{
    class ChatRepositoryInputValidator
    {
        private readonly string connectionString;
        private readonly UserRepository userRepository;
        private readonly AttachmentRepository attachmentRepository;
        private readonly ChatRepository chatRepository;
        private static readonly Logger logger = LogManager.GetLogger(typeof(ChatRepositoryInputValidator).FullName);

        public ChatRepositoryInputValidator(
            string connectionString,
            AttachmentRepository attachmentRepository,
            UserRepository userRepository,
            ChatRepository chatRepository)
        {
            this.connectionString = connectionString;
            this.attachmentRepository = attachmentRepository;
            this.userRepository = userRepository;
            this.chatRepository = chatRepository;
        }



        public bool ValidateChatInputAndAddData(Chat chat)
        {
            bool valid = CheckNull(chat) &&
                         CheckMembersExistensAndAddThemToChat(chat);
            if (!valid) return false;

            AddChatData(chat);

            if (chat.Type == ChatType.personal)
            {
                CleanExcessiveData(chat);
                var exists = SamePersonalChatExists(chat);
                if (exists) return false;
                else return true;
            }
            // ChatType == group
            else
            {
                return AdminIsChatMember(chat) &&
                       CheckRange(chat) &&
                       CheckAvatarExistens(chat.AvatarId);
            }
        }



        private bool CheckNull(Chat chat)
        {
            return chat         != null &&
                   chat.Members != null;
        }

        private bool CheckMembersExistensAndAddThemToChat(Chat chat)
        {
            var members = new List<User>();

            try
            {
                foreach (User member in chat.Members)
                    members.Add(userRepository.GetById(member.Id));
            }
            catch (SqlException ex)
            {
                var chatData = $"AdminId = [{chat.AdminId}], " +
                               $"Name = [{chat.Name}], " +
                               $"AvatarId = [{chat.AvatarId}], " +
                               $"{chatRepository.RepresentChatMembersAsStringOfIds(chat)}";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), chatData, logger);
            }

            foreach (User member in members)
                if (member == null) return false;

            chat.Members = members.AsReadOnly();

            return true;
        }

        /// <summary>
        /// Sets chat's Id and Type. 
        /// </summary>
        /// <param name="chat"> Chat data </param>
        private void AddChatData(Chat chat)
        {
            // Set chat Id
            chat.Id = Guid.NewGuid();

            // Set chat Type
            if (chat.Members.Count > 2) chat.Type = ChatType.group;
            else chat.Type = ChatType.personal;
        }

        private void CleanExcessiveData(Chat chat)
        {
            chat.AdminId = Guid.Empty;
            chat.Name = null;
            chat.AvatarId = Guid.Empty;
        }

        /// <summary>
        /// Verifies that there are personal chats with same 1 or 2 members.
        /// If such chat found, returns information about inability to create new chat.
        /// </summary>
        /// <param name="chat"> Chat to find </param>
        /// <returns>
        /// True: chat with same members exists
        /// False: chat with same members does not exists
        /// </returns>
        private bool SamePersonalChatExists(Chat chat)
        { 
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    if (chat.Members.Count == 1) return OnePersonChatExists(command, chat);
                    else return TwoPersonsChatExists(command, chat);
                }
            }
        }

        private bool OnePersonChatExists(SqlCommand command, Chat chat)
        {
            try
            {
                command.CommandText = Properties.Resources.OnePersonChatExists;
                command.Parameters.AddWithValue("@user", chat.Members[0].Id);
                command.ExecuteNonQuery();

                string result = "";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetString(reader.GetOrdinal("RESULT"));
                    }
                }

                if (result == "TRUE") return true;
                else return false;
            }
            catch (SqlException ex)
            {
                var chatData = $"AdminId = [{chat.AdminId}], " +
                               $"Name = [{chat.Name}], " +
                               $"AvatarId = [{chat.AvatarId}], " +
                               $"{chatRepository.RepresentChatMembersAsStringOfIds(chat)}";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), chatData, logger);
            }

            // Unreachable
            return true;
        }

        private bool TwoPersonsChatExists(SqlCommand command, Chat chat)
        {
            try
            {
                command.CommandText = Properties.Resources.TwoPersonsChatExists;
                command.Parameters.AddWithValue("@user1", chat.Members[0].Id);
                command.Parameters.AddWithValue("@user2", chat.Members[1].Id);
                command.ExecuteNonQuery();

                string result = "";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetString(reader.GetOrdinal("RESULT"));
                    }
                }

                if (result == "TRUE") return true;
                else return false;
            }
            catch (SqlException ex)
            {
                var chatData = $"AdminId = [{chat.AdminId}], " +
                               $"Name = [{chat.Name}], " +
                               $"AvatarId = [{chat.AvatarId}], " +
                               $"{chatRepository.RepresentChatMembersAsStringOfIds(chat)}";

                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), chatData, logger);
            }

            // Unreachable
            return true;
        }

        private bool AdminIsChatMember(Chat chat)
        {
            bool adminIsChatMember = false;

            foreach (User member in chat.Members)
            {
                if (member.Id == chat.AdminId)
                {
                    adminIsChatMember = true;
                    break;
                }
            }

            return adminIsChatMember;
        }

        private bool CheckRange(Chat chat)
        {
            if (chat.Name == null) return false;

            return chat.Name.Length >= ChatInputConstraints.MinChatNameLength &&
                   chat.Name.Length <= ChatInputConstraints.MaxChatNameLength;
        }

        private bool CheckAvatarExistens(Guid avatarId)
        {
            if (avatarId == Guid.Empty) return true;

            Attachment foundAttachment = null;

            try { foundAttachment = attachmentRepository.Get(avatarId); }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"Id = [{avatarId}]", logger);
            }

            if (foundAttachment == null) return false;
            else return true;
        }
    }
}
