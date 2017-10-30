using Messenger.Model;
using System;

namespace Messenger.DataLayer
{
    public interface IUserRepository
    {
        User Create(User user);
        void Delete(Guid id);
        User Get(Guid id);
    }
}
