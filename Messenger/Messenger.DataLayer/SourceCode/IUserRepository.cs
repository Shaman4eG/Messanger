using Messenger.Model;
using System;

namespace Messenger.DataLayer
{
    public interface IUserRepository
    {
        User Create(User user);
        User Get(Guid userId);
        bool? Update(User user);
        bool Delete(Guid userId);
    }
}
