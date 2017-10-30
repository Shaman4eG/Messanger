using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Messenger.DataLayer
{
    public interface IChatRepository
    {
        Chat Create(Chat chat);
        void Delete(Guid chatId);
        Chat Get(Guid chatId);
        ReadOnlyCollection<Chat> GetUserChats(Guid userId);
    }
}
