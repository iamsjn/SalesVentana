using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SalesVentana.Data
{
    public interface IDbFactory
    {
        Configuration ConfigManager { get; }
        SalesVentanaConnection Initialize();
        void Terminate();
    }
}
