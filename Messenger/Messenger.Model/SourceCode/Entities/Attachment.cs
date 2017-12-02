using System;

namespace Messenger.Model
{
    // TODO: Add AttachmentType class to hardset them (png, jpg, mp4, etc.)
    public class Attachment
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public byte[] File { get; set; }
    }
}
