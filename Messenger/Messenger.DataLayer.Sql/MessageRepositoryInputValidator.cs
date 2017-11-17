using System;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class MessageRepositoryInputValidator
    {
        private readonly UserRepository userRepository;
        private readonly ChatRepository chatRepository;

        public MessageRepositoryInputValidator(UserRepository userRepository, ChatRepository chatRepository)
        {
            this.userRepository = userRepository;
            this.chatRepository = chatRepository;
        }



        public bool ValidateSend(Message message)
        {
            return (
                        CheckNull(message)           &&
                        CheckChatExistens(message)   &&
                        CheckAuthorExistens(message) &&
                        CheckRange(message)
                    );
        }



        private bool CheckNull(Message message)
        {
            return (
                        message          != null       &&
                        message.ChatId   != Guid.Empty &&
                        message.AuthorId != Guid.Empty &&
                        (message.Text != null || message.AttachmentId != Guid.Empty) 
                    );
        }

        private bool CheckChatExistens(Message message)
        {
            return chatRepository.Get(message.ChatId) != null; 
        }

        private bool CheckAuthorExistens(Message message)
        {
            return userRepository.Get(message.AuthorId) != null;
        }

        private bool CheckRange(Message message)
        {
            if (message.AttachmentId == Guid.Empty &&
                message.Text == null)
            {
                return false;
            }

            if (message.Text == null) return true;
            else
            {
                return (
                            message.Text.Length >= InputConstraintsAndDefaultValues.MinMessageTextLength &&
                            message.Text.Length <= InputConstraintsAndDefaultValues.MaxMessageTextLength
                        );
            }
    }


    }
}
