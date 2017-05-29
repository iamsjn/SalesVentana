using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesVentana.BO;
using System.Linq.Expressions;
using System.Data;

namespace SalesVentana.Data
{
    public interface IPurchaseOrderRepository
    {
        DataSet GetPurchaseOrderDetail();
    }
}
