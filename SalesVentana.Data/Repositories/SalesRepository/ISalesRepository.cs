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
    public interface ISalesRepository
    {
        DataTable GetYearlySales(int year);
        DataTable GetProductCategory(string brandIds);
        DataTable GetBrand();
        DataTable GetProduct(string categoryIds);
    }
}
