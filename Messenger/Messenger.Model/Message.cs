using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Model
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid AuthorId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public Guid AttachmentId { get; set; }
        public bool SelfDeletion { get; set; }
    }
}
