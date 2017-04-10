using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SalesVentana.Data
{
    public static class UserExtensions
    {
        public static User GetSingleByUsername(this IBaseRepository<User> userRepository, string username)
        {
            return userRepository.GetAll<UserMapper>("Sales_User").FirstOrDefault(x => x.Username == username);
        }
    }
}
