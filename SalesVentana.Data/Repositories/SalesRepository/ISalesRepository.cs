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
        DataTable GetYearlySales(int year, string reportType, string brandIds, string categoryIds, string productIds, string regionIds, string channelIds);
        DataTable GetProductCategory(string brandIds);
        DataTable GetBrand();
        DataTable GetProduct(string categoryIds);
        DataTable GetRegion();
        DataTable GetChannel();
    }
}
