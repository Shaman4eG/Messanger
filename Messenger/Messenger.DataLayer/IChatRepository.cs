using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.DataLayer
{
    public interface IChatRepository
    {
        Chat Create(ReadOnlyCollection<Guid> members, Guid adminId, string name = "", byte[] avatar = null);
        void Delete(Guid chatId);
        Chat Get(Guid chatId);
        IEnumerable<Chat> GetUserChats(Guid userId);
    }
}
