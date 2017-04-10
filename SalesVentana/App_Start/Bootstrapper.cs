using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SalesVentana
{
    public class Bootstrapper
    {
        public static void Run()
        {
            //configure Autofac
            AutofacWebapiConfig.Initialize(GlobalConfiguration.Configuration);

            //Configure AutoMapper
            //AutoMapperConfiguration.Configure();
        }
    }
}