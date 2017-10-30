using System;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class MessageRepositoryInputValidator
    {
        public bool ValidateSend(Message message)
        {
            return (
                        ValidateCreate_CheckNull(message) &&
                        ValidateCreate_CheckRange(message)
                    );
        }

        private bool ValidateCreate_CheckNull(Message message)
        {
            return (
                        message        != null &&
                        message.Author != null &&
                        message.ChatId != null &&
                        (message.Text != null || message.AttachmentId != Guid.Empty)
                    );
        }

        private bool ValidateCreate_CheckRange(Message message)
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
                            message.Text.Length >= InputRestrictionsAndDefaultValues.MinMessageTextLength &&
                            message.Text.Length <= InputRestrictionsAndDefaultValues.MaxMessageTextLength
                        );
            }
    }


    }
}
