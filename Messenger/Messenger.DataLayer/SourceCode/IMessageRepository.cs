using Messenger.Model;
using System;
using System.Collections.ObjectModel;

namespace Messenger.DataLayer
{
    public interface IMessageRepository
    {
        Message Send(Message message);
        Message Get(Guid messageId);
        ReadOnlyCollection<Message> GetChatMessages(Guid chatId);
        ReadOnlyCollection<Message> GetNewChatMessages(Guid chatId, int currentNumberOfFetchedMessages);
        bool Delete(Guid messageId);
    }
}
