using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public interface IMapperBase<T> where T : class, IBasicObjectBase, new()
    {
        T MapObject(IDataReader dr);
    }
}
