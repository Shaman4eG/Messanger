using System;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class AttachmentRepositoryInputValidator
    {
        public bool ValidateInput(Attachment attachment)
        {
            return (
                        CheckNull(attachment) &&
                        CheckRange(attachment)
                    );
        }

        private bool CheckNull(Attachment attachment)
        {
            return (
                        attachment      != null &&
                        attachment.Type != null &&
                        attachment.File != null
                    );
        }

        private bool CheckRange(Attachment attachment)
        {
            return (
                        attachment.File.Length >= InputConstraintsAndDefaultValues.MinFileSize       &&
                        attachment.File.Length <= InputConstraintsAndDefaultValues.MaxFileSize       &&
                        attachment.Type.Length >= InputConstraintsAndDefaultValues.MinFileTypeLenght &&
                        attachment.Type.Length <= InputConstraintsAndDefaultValues.MaxFileTypeLength
                    );
        }
    }
}
