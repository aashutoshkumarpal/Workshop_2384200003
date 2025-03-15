using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        User? GetUserByEmail(string email);
        void AddUser(User user);

        void UpdateUser(User user);
    }
}
