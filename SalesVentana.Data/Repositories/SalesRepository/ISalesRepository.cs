﻿using System;
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
        DataTable GetYearlySales(int year, string reportType, string brandIds, 
            string categoryIds, string productIds, string regionIds, string channelIds, 
            string showroomIds);
        DataTable GetProductCategory(string brandIds);
        DataTable GetBrand();
        DataTable GetProduct(string categoryIds);
        DataTable GetRegion();
        DataTable GetChannel();
        DataTable GetShowroom();
        DataTable GetQuaterlySales(int year, string salesQuarter, string reportType, 
            string brandIds, string categoryIds, string productIds, string regionIds, 
            string channelIds, string showroomIds);
    }
}
