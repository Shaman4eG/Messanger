using System;
using System.Collections.ObjectModel;

namespace Messenger.Model
{
    public class Chat
    {
        public Guid Id { get; set; }
        public ChatType Type { get; set; }
        public Guid AdminId { get; set; }
        public string Name { get; set; }
        public Guid AvatarId { get; set; }
        public ReadOnlyCollection<User> Members;
    }
}
