using Messenger.Model;
using System;
using System.Collections.ObjectModel;

namespace Messenger.DataLayer
{
    public interface IChatRepository
    {
        Chat Create(Chat chat);
        Chat Get(Guid chatId);
        bool Delete(Guid chatId);
        ReadOnlyCollection<Chat> GetUserChats(Guid userId);
    }
}
