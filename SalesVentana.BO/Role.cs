using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.BO
{
    public class Role : IBasicObjectBase
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
