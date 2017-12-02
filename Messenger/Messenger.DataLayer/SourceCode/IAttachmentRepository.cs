using Messenger.Model;
using System;

namespace Messenger.DataLayer
{
    public interface IAttachmentRepository
    {
        Attachment Create(Attachment attachment);
        bool Delete(Guid attachmentId);
        Attachment Get(Guid atttachmentId);
    }
}
