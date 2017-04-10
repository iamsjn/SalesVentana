using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class RoleMapper : IMapperBase<Role>
    {
        private Role _role = null;
        public Role MapObject(IDataReader dr)
        {
            _role = new Role();

            _role.ID = dr.GetInt32(0);
            _role.Name = dr.GetString(1);
            return _role;
        }
    }
}
