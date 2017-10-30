using System;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class ChatRepositoryInputValidator
    {
        public bool ValidateCreateAndAddInfo(Chat chat)
        {
            bool valid = ValidateCreate_CheckNull(chat);
            if (!valid) return false;

            AddChatInfo(chat);

            if (chat.Type == ChatType.group)
            {
                return ValidateCreate_AdminIsChatMember(chat) &&
                       ValidateCreate_CheckRange(chat);
            }
            else return ValidateCreate_CheckRange(chat);
        }

        private bool ValidateCreate_CheckNull(Chat chat)
        {
            return (
                        chat         != null &&
                        chat.AdminId != null &&
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
                chat.Name = InputRestrictionsAndDefaultValues.DefaultChatName;
            }

            // Set chat Avatar
            if (chat.Type == ChatType.group &&
                chat.Avatar == null)
            {
                chat.Avatar = InputRestrictionsAndDefaultValues.DefaultUserAvatar;
            }

            // Remove chat Admin in personal chat
            if (chat.Type == ChatType.personal) chat.AdminId = Guid.Empty;
        }

        private bool ValidateCreate_AdminIsChatMember(Chat chat)
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

        private bool ValidateCreate_CheckRange(Chat chat)
        {
            if (chat.Name == null) return true;

            else return (
                            chat.Name.Length >= InputRestrictionsAndDefaultValues.MinUserNameLength &&
                            chat.Name.Length <= InputRestrictionsAndDefaultValues.MaxUserNameLength 
                        );
        }


    }
}
