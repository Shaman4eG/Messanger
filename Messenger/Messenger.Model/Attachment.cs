using System;
using System.Collections.ObjectModel;

namespace Messenger.Model
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public byte[] File { get; set; }
    }
}
