using System;
using Messenger.Model;
using System.Data.SqlClient;
using System.Reflection;
using NLog;
using Messenger.Constants;

namespace Messenger.DataLayer.Sql
{
    class MessageRepositoryInputValidator
    {
        private readonly UserRepository userRepository;
        private readonly ChatRepository chatRepository;
        private readonly AttachmentRepository attachmentRepository;
        private static readonly Logger logger = LogManager.GetLogger(typeof(UserRepository).FullName);

        public MessageRepositoryInputValidator(
            UserRepository userRepository, 
            ChatRepository chatRepository,
            AttachmentRepository attachmentRepository)
        {
            this.userRepository = userRepository;
            this.chatRepository = chatRepository;
            this.attachmentRepository = attachmentRepository;
        }



        public bool ValidateMessageInput(Message message)
        {
            return CheckNull(message) &&
                   CheckChatExistens(message) &&
                   CheckAuthorExistens(message) &&
                   CheckAttachmentAndText(message);
        }



        private bool CheckNull(Message message)
        {
            return message          != null &&
                   message.ChatId   != Guid.Empty &&
                   message.AuthorId != Guid.Empty &&
                   (message.Text != null || message.AttachmentId != Guid.Empty);
    }

        private bool CheckChatExistens(Message message)
        {
            try { return chatRepository.Get(message.ChatId) != null; }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"chatId = [{message.ChatId}]", logger);
            }

            // Unrechable
            return false;
        }

        private bool CheckAuthorExistens(Message message)
        {
            try { return userRepository.GetById(message.AuthorId) != null; }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"authorId = [{message.AuthorId}]", logger);
            }

            // Unrechable
            return false;
        }

        private bool CheckAttachmentAndText(Message message)
        {
            bool attachmentFound = true;
            bool textFound = true;

            if (message.AttachmentId != Guid.Empty) attachmentFound = CheckAttachmentExistens(message);

            // TODO: Additional checks
            if (message.Text != null && message.Text != "") textFound = CheckTextRange(message);

            return attachmentFound && textFound;
        }

        private bool CheckAttachmentExistens(Message message)
        {
            try { return attachmentRepository.Get(message.AttachmentId) == null ? false : true; }
            catch (SqlException ex)
            {
                Utility.SqlExceptionHandler(ex, MethodBase.GetCurrentMethod(), $"attachmentId = [{message.AttachmentId}]", logger);
            }

            // Unreachable
            return false;
        }

        private bool CheckTextRange(Message message)
        {
            return message.Text.Length >= MessageInputConstraints.MinMessageTextLength &&
                   message.Text.Length <= MessageInputConstraints.MaxMessageTextLength;
        }


    }
}
