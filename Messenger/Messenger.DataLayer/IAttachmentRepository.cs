using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Messenger.DataLayer
{
    public interface IAttachmentRepository
    {
        Attachment Create(Attachment attachment);
        void Delete(Guid attachmentId);
        Attachment Get(Guid atttachmentId);
    }
}
