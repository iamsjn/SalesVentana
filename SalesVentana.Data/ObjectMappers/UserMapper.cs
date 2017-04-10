using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class UserMapper : IMapperBase<User>
    {
        private User _user = null;
        public User MapObject(IDataReader dr)
        {
            _user = new User();
            _user.ID = dr.GetInt32(0);
            _user.Username = dr.GetString(1);
            _user.Email = dr.GetString(2);
            _user.HashedPassword = dr.GetString(3);
            _user.Salt = dr.GetString(4);
            _user.IsLocked = dr.GetBoolean(5);
            _user.DateCreated = dr.GetDateTime(6);
            return _user;
        }
    }
}
