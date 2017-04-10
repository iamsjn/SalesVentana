using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public interface IUnitOfWork
    {
        void Terminate();
    }
}
