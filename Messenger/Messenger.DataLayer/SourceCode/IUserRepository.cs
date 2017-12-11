using Messenger.Model;
using System;

namespace Messenger.DataLayer
{
    public interface IUserRepository
    {
        User Create(User user);
        User GetById(Guid userId);
        User GetByEmail(string email);
        bool? Update(User user);
        bool Delete(Guid userId);
    }
}
