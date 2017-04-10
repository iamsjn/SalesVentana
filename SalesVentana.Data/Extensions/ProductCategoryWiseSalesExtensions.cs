using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public static class ProductCategoryWiseSalesExtensions
    {
        public static DataTable GetProductCategoryWiseYearlySales(this ISalesRepository productCategoryWiseSalesRepository)
        {
            return new DataTable();
        }
    }
}
