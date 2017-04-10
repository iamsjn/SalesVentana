using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class UserRoleMapper : IMapperBase<UserRole>
    {
        private UserRole _userRole = null;
        public UserRole MapObject(IDataReader dr)
        {
            _userRole = new UserRole();

            _userRole.ID = dr.GetInt32(0);
            _userRole.UserId = dr.GetInt32(1);
            _userRole.RoleId = dr.GetInt32(2);
            return _userRole;
        }
    }
}
