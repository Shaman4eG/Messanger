using Messenger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.DataLayer
{
    public interface IUserRepository
    {
        User Create(User user);
        void Delete(Guid id);
        User Get(Guid id);
    }
}
