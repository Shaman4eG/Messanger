using Messenger.Constants;
using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class AttachmentRepositoryInputValidator
    {
        public bool ValidateAttachmentInput(Attachment attachment)
        {
            return CheckNull(attachment) &&
                   CheckRange(attachment);
        }

        private bool CheckNull(Attachment attachment)
        {
            return attachment      != null &&
                   attachment.Type != null &&
                   attachment.File != null;
        }

        private bool CheckRange(Attachment attachment)
        {
            return attachment.Type.Length >= AttachmentInputConstraints.MinFileTypeLenght &&
                   attachment.Type.Length <= AttachmentInputConstraints.MaxFileTypeLength &&
                   attachment.File.Length >= AttachmentInputConstraints.MinFileSize &&
                   attachment.File.Length <= AttachmentInputConstraints.MaxFileSize;
        }
    }
}
