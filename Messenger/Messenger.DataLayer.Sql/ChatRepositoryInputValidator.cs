using System;
using Messenger.Model;
using System.Data.SqlClient;
using System.IO;

namespace Messenger.DataLayer.Sql
{
    public class ChatRepositoryInputValidator
    {
        private readonly string connectionString;
        private readonly UserRepository userRepository;

        public ChatRepositoryInputValidator(string connectionString, UserRepository userRepository)
        {
            this.connectionString = connectionString; 
            this.userRepository = userRepository;
        }



        public bool ValidateCreateAndAddInfo(Chat chat)
        {
            bool valid = (
                                CheckNull(chat) &&
                                CheckMembersExistens(chat)
                         );
            if (!valid) return false;

            AddChatInfo(chat);

            valid = CheckRange(chat);
            if (!valid) return false;

            if (chat.Type == ChatType.personal)
            {
                var exists = SamePersonalChatExists(chat);
                if (exists) return false;
                else return true;
            }
            else return AdminIsChatMember(chat);
        }



        private bool CheckNull(Chat chat)
        {
            return (
                        chat         != null &&
                        chat.Members != null 
                    );
        }

        private void AddChatInfo(Chat chat)
        {
            // Set chat Id
            chat.Id = Guid.NewGuid();

            // Set chat Type
            if (chat.Members.Count > 2) chat.Type = ChatType.group;
            else chat.Type = ChatType.personal;

            // Set chat Name
            if (chat.Type == ChatType.group &&
                chat.Name == null)
            {
                chat.Name = InputConstraintsAndDefaultValues.DefaultChatName;
            }

            // Set chat Avatar
            if (chat.Type == ChatType.group &&
                chat.Avatar == null)
            {
                chat.Avatar = InputConstraintsAndDefaultValues.DefaultUserAvatar;
            }

            // Remove chat Admin in personal chat
            if (chat.Type == ChatType.personal) chat.AdminId = Guid.Empty;
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
            if (chat.Name != null)
            {
                if (chat.Name.Length < InputConstraintsAndDefaultValues.MinUserNameLength &&
                    chat.Name.Length > InputConstraintsAndDefaultValues.MaxUserNameLength)
                {
                    return false;
                }
            }

            if (chat.Avatar != null)
            {
                if (chat.Avatar.Length > InputConstraintsAndDefaultValues.MaxImageSize)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckMembersExistens(Chat chat)
        {
            foreach (User member in chat.Members)
                if (userRepository.Get(member.Id) == null) return false;

            return true;
        }

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

        //TODO: Log exception
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
            catch (Exception ex)
            {
                // log "Problems openning file: " + ex.Message
                return true;
            }
        }

        //TODO: Log exception
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
            catch (Exception ex)
            {
                // log "Problems openning file: " + ex.Message
                return true;
            }
        }
    }
}
