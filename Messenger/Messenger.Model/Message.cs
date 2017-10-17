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
        public User Author { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public byte[] File { get; set; }
        public bool SelfDelition { get; set; }
    }
}
